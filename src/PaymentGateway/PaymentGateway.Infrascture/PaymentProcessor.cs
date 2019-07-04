using System;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using Polly;

namespace PaymentGateway.Infrastructure
{
    /// <inheritdoc cref="IProcessPayment" />
    public class PaymentProcessor : IProcessPayment
    {
        private readonly IThrowsException _gatewayExceptionSimulator;
        private readonly ILogger<PaymentProcessor> _logger;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;
        private readonly IProvideTimeout _timeoutProviderForBankResponseWaiting;

        public PaymentProcessor(IEventSourcedRepository<Payment> paymentsRepository, ILogger<PaymentProcessor> logger,
            IProvideTimeout timeoutProviderForBankResponseWaiting,
            IThrowsException gatewayExceptionSimulator = null)
        {
            _paymentsRepository = paymentsRepository;
            _logger = logger;
            _timeoutProviderForBankResponseWaiting = timeoutProviderForBankResponseWaiting;
            _gatewayExceptionSimulator = gatewayExceptionSimulator;
        }

        public async Task<PaymentResult> AttemptPaying(IAdaptToBank bankAdapter, Payment payment)
        {
            var payingAttempt = payment.MapToAcquiringBank();
            IBankResponse bankResponse = new NullBankResponse();

            // Connection to bank
            var policy = Policy.Handle<TaskCanceledException>()
                .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(Math.Pow(2, retry)));

            var policyResult = await policy.ExecuteAndCaptureAsync(async () =>
            {
                using (var cts = new CancellationTokenSource())
                {
                    var timeout = _timeoutProviderForBankResponseWaiting.GetTimeout();
                    cts.CancelAfter(timeout);

                    bankResponse = await bankAdapter.RespondToPaymentAttempt(payingAttempt, cts.Token);
                }
            });

            if (policyResult.FinalException != null)
            {
                _logger.LogError($"Payment gatewayId='{payingAttempt.GatewayPaymentId}' requestId='{payingAttempt.PaymentRequestId}' Timeout");

                payment.Timeout();
                await _paymentsRepository.Save(payment, payment.Version);

                return PaymentResult.Fail(payingAttempt.GatewayPaymentId, payingAttempt.PaymentRequestId, policyResult.FinalException, "Timeout");
            }

            var strategy = Build(bankResponse, _paymentsRepository);

            await strategy.Handle(_gatewayExceptionSimulator, payingAttempt.GatewayPaymentId);

            return PaymentResult.Finished(payingAttempt.GatewayPaymentId, payingAttempt.PaymentRequestId);
        }

        private static IHandleBankResponseStrategy Build(IBankResponse bankResponse, IEventSourcedRepository<Payment> paymentsRepository)
        {
            switch (bankResponse)
            {
                case BankResponse response:
                    return new RespondedBankStrategy(response, paymentsRepository);

                case BankDoesNotRespond _:
                    return new NotRespondedBankStrategy(paymentsRepository);
            }

            throw new ArgumentException();
        }
    }
}