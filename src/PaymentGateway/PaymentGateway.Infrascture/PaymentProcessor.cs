using System;
using System.Threading;
using System.Threading.Tasks;
using PaymentGateway.Domain;
using PaymentGateway.Domain.AcquiringBank;
using SimpleCQRS;

namespace PaymentGateway.Infrastructure
{

    /// <summary>
    /// Glue component which calls bank facade <see cref="ITalkToAcquiringBank"/>.
    /// Then do necessary changes in PaymentGateway's domain.
    /// </summary>
    public class PaymentProcessor : IProcessPayment
    {
        private readonly ITalkToAcquiringBank _acquiringBankFacade;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;
        private readonly SimulateGatewayException _gatewayExceptionSimulator;

        public PaymentProcessor(ITalkToAcquiringBank acquiringBankFacade, IEventSourcedRepository<Payment> paymentsRepository, SimulateGatewayException gatewayExceptionSimulator = null)
        {
            _acquiringBankFacade = acquiringBankFacade;
            _paymentsRepository = paymentsRepository;
            _gatewayExceptionSimulator = gatewayExceptionSimulator;
        }

        public async Task AttemptPaying(PayingAttempt payingAttempt)
        {
            var bankResponse = await _acquiringBankFacade.Pay(payingAttempt);
            IStrategy strategy = Build(bankResponse, _paymentsRepository);

            await strategy.Handle(_gatewayExceptionSimulator, payingAttempt.GatewayPaymentId);

           
        }

        private static IStrategy Build(IBankResponse bankResponse, IEventSourcedRepository<Payment> paymentsRepository)
        {
            switch (bankResponse)
            {
                case BankResponse response:
                    return new RespondedBank(response, paymentsRepository);

                case BankDoesNotRespond noResponse:
                    return new NotRespondedBank(noResponse, paymentsRepository);
            }

            throw new ArgumentException();
        }
    }

    internal class NotRespondedBank : IStrategy
    {
        private readonly BankDoesNotRespond _noResponse;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;

        public NotRespondedBank(BankDoesNotRespond noResponse, IEventSourcedRepository<Payment> paymentsRepository)
        {
            _noResponse = noResponse;
            _paymentsRepository = paymentsRepository;
        }

        public async Task Handle(SimulateGatewayException gatewayExceptionSimulator, Guid gatewayPaymentId)
        {
            var knownPayment = await _paymentsRepository.GetById(gatewayPaymentId);

            knownPayment.BankConnectionFails();

            await _paymentsRepository.Save(knownPayment, knownPayment.Version);
        }
    }

    internal class RespondedBank : IStrategy
    {
        private readonly BankResponse _bankResponse;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;

        public RespondedBank(BankResponse response, IEventSourcedRepository<Payment> paymentsRepository)
        {
            _bankResponse = response;
            _paymentsRepository = paymentsRepository;
        }

        public async Task Handle(SimulateGatewayException gatewayExceptionSimulator, Guid gatewayPaymentId)
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
                    //TODO : log
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

    public interface IStrategy
    {
        Task Handle(SimulateGatewayException gatewayExceptionSimulator, Guid payingAttemptGatewayPaymentId);
    }

    public class SimulateGatewayException
    {
        public void Throws()
        {
            throw new FakeException();
        }

        private class FakeException : Exception { }
    }
}