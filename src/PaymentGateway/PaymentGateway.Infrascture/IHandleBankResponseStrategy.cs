using System;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    /// <summary>
    ///     Before forwarding <see cref="PaymentRequest" /> to Acquiring bank, we should be able to connect to that bank's API.
    ///     We handle differently when we can or can't connect to bank's API.
    /// </summary>
    public interface IHandleBankResponseStrategy
    {
        Task Handle(IThrowsException gatewayExceptionSimulator, Guid payingAttemptGatewayPaymentId);
    }

    /// <summary>
    ///     <see cref="PaymentRequest"/> handling strategy when we failed to connect to bank's API
    /// </summary>
    internal class NotRespondedBankStrategy : IHandleBankResponseStrategy
    {
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;

        public NotRespondedBankStrategy(IEventSourcedRepository<Payment> paymentsRepository)
        {
            _paymentsRepository = paymentsRepository;
        }

        public async Task Handle(IThrowsException gatewayExceptionSimulator, Guid gatewayPaymentId)
        {
            var knownPayment = await _paymentsRepository.GetById(gatewayPaymentId);

            knownPayment.BankConnectionFails();

            await _paymentsRepository.Save(knownPayment, knownPayment.Version);
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     <see cref="T:PaymentGateway.Infrastructure.PaymentRequest" /> handling strategy when we do have connected to bank's API
    /// </summary>
    public class RespondedBankStrategy : IHandleBankResponseStrategy
    {
        private readonly BankResponse _bankResponse;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;

        public RespondedBankStrategy(BankResponse response, IEventSourcedRepository<Payment> paymentsRepository)
        {
            _bankResponse = response;
            _paymentsRepository = paymentsRepository;
        }

        public async Task Handle(IThrowsException gatewayExceptionSimulator, Guid gatewayPaymentId)
        {
            Payment knownPayment = null;
            var bankPaymentId = _bankResponse.BankPaymentId;
            try
            {
                try
                {
                    knownPayment = await _paymentsRepository.GetById(gatewayPaymentId);
                    gatewayExceptionSimulator?.Throws();
                }
                catch (AggregateNotFoundException)
                {
                    return;
                }

                switch (_bankResponse.PaymentStatus)
                {
                    case BankPaymentStatus.Accepted:
                        knownPayment.AcceptPayment(bankPaymentId);
                        break;
                    case BankPaymentStatus.Rejected:
                        knownPayment.BankRejectPayment(bankPaymentId);
                        break;
                }

                await _paymentsRepository.Save(knownPayment, knownPayment.Version);
            }

            catch (Exception)
            {
                //TODO: log
                knownPayment.FailOnGateway(bankPaymentId);
                await _paymentsRepository.Save(knownPayment, knownPayment.Version);
            }
        }
    }
}