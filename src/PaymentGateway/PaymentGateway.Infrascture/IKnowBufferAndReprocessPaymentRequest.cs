using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using Polly;
using Polly.Wrap;

namespace PaymentGateway.Infrastructure
{
    public interface IKnowBufferAndReprocessPaymentRequest
    {
        void Buffer(IAdaptToBank bankAdapter, Payment payment);
        void ProcessBufferedPaymentRequest();
    }

    public class PaymentRequestsLaterHandler : IKnowBufferAndReprocessPaymentRequest
    {
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;
        private readonly IProvideTimeout _timeoutProviderForBankResponseWaiting;
        private readonly ILogger<PaymentRequestsLaterHandler> _bankResponseProcessingLogger;
        private readonly IAmCircuitBreakers _circuitBreakers;
        private readonly IThrowsException _gatewayExceptionSimulator;
        private readonly ConcurrentQueue<PaymentRequestBuffer> _buffer = new ConcurrentQueue<PaymentRequestBuffer>();

        public PaymentRequestsLaterHandler(IEventSourcedRepository<Payment> paymentsRepository,
                                        IProvideTimeout timeoutProviderForBankResponseWaiting,
                                        ILogger<PaymentRequestsLaterHandler> bankResponseProcessingLogger,
                                        IAmCircuitBreakers circuitBreakers,
                                        IThrowsException gatewayExceptionSimulator = null)
        {
            _paymentsRepository = paymentsRepository;
            _timeoutProviderForBankResponseWaiting = timeoutProviderForBankResponseWaiting;
            _bankResponseProcessingLogger = bankResponseProcessingLogger;
            _circuitBreakers = circuitBreakers;
            _gatewayExceptionSimulator = gatewayExceptionSimulator;
        }

        public void Buffer(IAdaptToBank bankAdapter,  Payment payment)
        {
            var payingAttempt = payment.MapToAcquiringBank();

            _bankResponseProcessingLogger.LogInformation($"<------ Enqueue request {payingAttempt.PaymentRequestId}");

            _buffer.Enqueue(new PaymentRequestBuffer(bankAdapter, payingAttempt, payment));
        }

        public void ProcessBufferedPaymentRequest()
        {
            while (!_buffer.IsEmpty)
            {
                if (_buffer.TryDequeue(out var bufferItem))
                {
                    bool success = AttemptPaying(bufferItem.BankAdapter, bufferItem.Payment).Result;
                    if(success)
                    {
                        _bankResponseProcessingLogger.LogInformation($"------> Dequeued and Processed request {bufferItem.PayingAttempt.PaymentRequestId}");
                    }
                    else
                    {
                        Buffer(bufferItem.BankAdapter, bufferItem.Payment);
                    }
                }
            }
        }

        private async Task<bool> AttemptPaying(IAdaptToBank bankAdapter, Payment payment)
        {
            var payingAttempt = payment.MapToAcquiringBank();

            var circuitBreaker = CircuitBreaker(bankAdapter, payment, payingAttempt);

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
            else
            {
                return false;
            }
            
            var strategy = BankResponseHandleStrategyBuilder.Build(bankResponse, _paymentsRepository);

            await strategy.Handle(_gatewayExceptionSimulator, bankResponse.GatewayPaymentId);

            return true;
        }

        private CircuitBreaker CircuitBreaker(IAdaptToBank bankAdapter, Payment payment, PayingAttempt payingAttempt)
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

                        Buffer(bankAdapter,  payment);
                    },
                    onReset: context =>
                    {
#pragma warning disable 4014
                        // Fire and forget (Polly OnReset does not provide awaitable signature)
                        ProcessBufferedPaymentRequest();

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