

using System.Threading;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using AcquiringBanks.Stub;
    using Domain;

    /// <inheritdoc cref="IProcessPayment"/>>
    public class PaymentProcessor : IProcessPayment
    {
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;
        private readonly ILogger<PaymentProcessor> _logger;
        private readonly IProvideTimeout _timeoutProviderForBankResponseWaiting;
        private readonly SimulateGatewayException _gatewayExceptionSimulator;

        public PaymentProcessor(IEventSourcedRepository<Payment> paymentsRepository, ILogger<PaymentProcessor> logger,
            IProvideTimeout timeoutProviderForBankResponseWaiting,
            SimulateGatewayException gatewayExceptionSimulator = null )
        {
            _paymentsRepository = paymentsRepository;
            _logger = logger;
            _timeoutProviderForBankResponseWaiting = timeoutProviderForBankResponseWaiting;
            _gatewayExceptionSimulator = gatewayExceptionSimulator;
        }

        public async Task<PaymentResult> AttemptPaying(IAdaptToBank bankAdapter, Payment payment )
        {

            var payingAttempt = payment.MapToAcquiringBank();
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(_timeoutProviderForBankResponseWaiting.GetTimeout()))
            {
                try
                {
                    var bankResponse = await bankAdapter.RespondToPaymentAttempt(payingAttempt);
                    if (timeoutCancellationTokenSource.Token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }

                    IHandleBankResponseStrategy strategy = Build(bankResponse, _paymentsRepository);

                    await strategy.Handle(_gatewayExceptionSimulator, payingAttempt.GatewayPaymentId);
                }
                catch (Exception ex)
                {
                    if (timeoutCancellationTokenSource.IsCancellationRequested && ex is TaskCanceledException)
                    {
                        _logger.LogError(
                            $"Payment gatewayId='{payingAttempt.GatewayPaymentId}' requestId='{payingAttempt.PaymentRequestId}' Timeout");

                        payment.Timeout();
                        await _paymentsRepository.Save(payment, payment.Version);

                        return PaymentResult.Fail(payingAttempt.GatewayPaymentId, payingAttempt.PaymentRequestId, ex,
                            "Timeout");
                    }
                }
            }

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

    public interface IProvideTimeout
    {
        TimeSpan GetTimeout();
    }
}