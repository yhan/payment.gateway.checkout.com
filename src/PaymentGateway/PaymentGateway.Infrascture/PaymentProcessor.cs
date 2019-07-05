using System;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using Polly;
using Polly.Wrap;

namespace PaymentGateway.Infrastructure
{
    /// <inheritdoc cref="IProcessPayment" />
    public class PaymentProcessor : IProcessPayment
    {
        private readonly IThrowsException _gatewayExceptionSimulator;
        private readonly ILogger<PaymentProcessor> _logger;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;
        private readonly IProvideTimeout _timeoutProviderForBankResponseWaiting;
        private readonly IKnowBufferAndReprocessPaymentRequest _failureHandler;
        private readonly IAmCircuitBreakers _circuitBreakers;

        public PaymentProcessor(IEventSourcedRepository<Payment> paymentsRepository,
                                ILogger<PaymentProcessor> logger,
                                IProvideTimeout timeoutProviderForBankResponseWaiting,
                                IKnowBufferAndReprocessPaymentRequest failureHandler,
                                IAmCircuitBreakers circuitBreakers,
                                IThrowsException gatewayExceptionSimulator = null)
        {
            _paymentsRepository = paymentsRepository;
            _logger = logger;
            _timeoutProviderForBankResponseWaiting = timeoutProviderForBankResponseWaiting;
            _failureHandler = failureHandler;
            _circuitBreakers = circuitBreakers;
            _gatewayExceptionSimulator = gatewayExceptionSimulator;
        }

        public async Task<IPaymentRequestHandlingResult> AttemptPaying(IAdaptToBank bankAdapter, Payment payment)
        {
            var payingAttempt = payment.MapToAcquiringBank();

            var circuitBreaker = CircuitBreaker(bankAdapter, payment);

            IBankResponse bankResponse = new NullBankResponse();

            var policyResult = await circuitBreaker.Policy.ExecuteAndCaptureAsync(async () =>
            {
                using (var cts = new CancellationTokenSource())
                {
                    var timeout = _timeoutProviderForBankResponseWaiting.GetTimeout();
                    cts.CancelAfter(timeout);

                    bankResponse = await bankAdapter.RespondToPaymentAttempt(payingAttempt, cts.Token);
                }
            });

            if (policyResult.FinalException == null)
            {
                circuitBreaker.Reset();
            }
            else if (policyResult.FinalException is BankDuplicatedPaymentIdException paymentDuplicatedException)
            {
                _logger.LogError(paymentDuplicatedException.Message);

                payment.HandleBankPaymentIdDuplication();
                await _paymentsRepository.Save(payment, payment.Version);

                return PaymentRequestHandlingStatus.Fail(payingAttempt.GatewayPaymentId, payingAttempt.PaymentRequestId, policyResult.FinalException, "Bank Duplicated PaymentId");
            }

            var strategy = BankResponseHandleStrategyBuilder.Build(bankResponse, _paymentsRepository);

            await strategy.Handle(_gatewayExceptionSimulator, bankResponse.GatewayPaymentId);

            return PaymentRequestHandlingStatus.Finished(payingAttempt.GatewayPaymentId, payingAttempt.PaymentRequestId);
        }

        private CircuitBreaker CircuitBreaker(IAdaptToBank bankAdapter, Payment payment)
        {
            var bankAdapterType = bankAdapter.GetType();
            if (!_circuitBreakers.TryGet(bankAdapterType, out CircuitBreaker circuitBreaker))
            {
                circuitBreaker = MakeCircuitBreaker(bankAdapter, payment);
                _circuitBreakers.Add(bankAdapterType, circuitBreaker);
            }

            return circuitBreaker;
        }

        private CircuitBreaker MakeCircuitBreaker(IAdaptToBank bankAdapter, Payment payment)
        {
            var breaker = Policy
                .Handle<TaskCanceledException>()
                .Or<FailedConnectionToBankException>()
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromMilliseconds(40),
                    onBreak: (exception, timespan, context) =>
                    {
                        // When circuit breaker opens, buffer failed `PayingAttempt`

                        _failureHandler.Buffer(bankAdapter, payment);
                    },
                    onReset: context =>
                    {
#pragma warning disable 4014
                        // Fire and forget (Polly OnReset does not provide awaitable signature)
                        _failureHandler.ProcessBufferedPaymentRequest();

#pragma warning restore 4014
                    });

            AsyncPolicyWrap policy = Policy.Handle<TaskCanceledException>()
                .Or<FailedConnectionToBankException>()
                .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(Math.Pow(2, retry)))
                .WrapAsync(breaker);

            return new CircuitBreaker(breaker, policy);
        }

    }
}