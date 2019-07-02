using System;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBanks.API;
using PaymentGateway.Domain;
using SimpleCQRS;
using BankPaymentStatus = PaymentGateway.Domain.AcquiringBank.BankPaymentStatus;
using PayingAttempt = PaymentGateway.Domain.AcquiringBank.PayingAttempt;

namespace PaymentGateway.Infrastructure
{

    /// <summary>
    /// Glue component which calls bank facade <see cref="ITalkToAcquiringBank"/>.
    /// Then do necessary changes in PaymentGateway's domain.
    /// </summary>
    public class PaymentProcessor : IProcessPayment
    {
        private readonly IMapMerchantToBankAdapter _bankAdapterMapper;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;
        private readonly SimulateGatewayException _gatewayExceptionSimulator;

        public PaymentProcessor(IMapMerchantToBankAdapter bankAdapterMapper, IEventSourcedRepository<Payment> paymentsRepository, SimulateGatewayException gatewayExceptionSimulator = null)
        {
            _bankAdapterMapper = bankAdapterMapper;
            _paymentsRepository = paymentsRepository;
            _gatewayExceptionSimulator = gatewayExceptionSimulator;
        }

        public async Task AttemptPaying(PayingAttempt payingAttempt)
        {
            var bankResponse = await _bankAdapterMapper.FindBankAdapter(payingAttempt.MerchantId).RespondToPaymentAttempt(payingAttempt);
            IHandleBankResponseStrategy strategy = Build(bankResponse, _paymentsRepository);

            await strategy.Handle(_gatewayExceptionSimulator, payingAttempt.GatewayPaymentId);
        }

        private static IHandleBankResponseStrategy Build(IBankResponse bankResponse, IEventSourcedRepository<Payment> paymentsRepository)
        {
            switch (bankResponse)
            {
                case BankResponse response:
                    return new RespondedBankStrategy(response, paymentsRepository);

                case BankDoesNotRespond noResponse:
                    return new NotRespondedBankStrategy(noResponse, paymentsRepository);
            }

            throw new ArgumentException();
        }
    }

    internal class NotRespondedBankStrategy : IHandleBankResponseStrategy
    {
        private readonly BankDoesNotRespond _noResponse;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;

        public NotRespondedBankStrategy(BankDoesNotRespond noResponse, IEventSourcedRepository<Payment> paymentsRepository)
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

    internal class RespondedBankStrategy : IHandleBankResponseStrategy
    {
        private readonly BankResponse _bankResponse;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;

        public RespondedBankStrategy(BankResponse response, IEventSourcedRepository<Payment> paymentsRepository)
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
                    case AcquiringBanks.API.BankPaymentStatus.Accepted:
                        knownPayment.AcceptPayment(bankPaymentId);
                        break;
                    case AcquiringBanks.API.BankPaymentStatus.Rejected:
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

    public interface IHandleBankResponseStrategy
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