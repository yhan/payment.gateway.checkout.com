using AcquiringBanks.API;
using NSubstitute;
using PaymentGateway.API.ReadAPI;
using PaymentGateway.API.WriteAPI;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    public class PaymentCQRS
    {
        protected internal AcquiringBankFacade AcquiringBank;
        protected internal EventSourcedRepository<Payment> EventSourcedRepository;
        protected internal AcquiringBanksMediator PaymentProcessor;
        protected internal InMemoryPaymentIdsMapping PaymentIdsMapping;
        protected internal PaymentReadController ReadController;
        protected internal PaymentRequestsController RequestController;

        private PaymentCQRS(EventSourcedRepository<Payment> eventSourcedRepository, PaymentRequestsController requestController, PaymentReadController readController, InMemoryPaymentIdsMapping paymentIdsMapping, AcquiringBankFacade acquiringBank, AcquiringBanksMediator paymentProcessor)
        {
            EventSourcedRepository = eventSourcedRepository;
            RequestController = requestController;
            ReadController = readController;
            PaymentIdsMapping = paymentIdsMapping;
            AcquiringBank = acquiringBank;
            PaymentProcessor = paymentProcessor;
        }

        internal static PaymentCQRS Build(BankPaymentStatus paymentStatus)
        {
            var eventSourcedRepository = new EventSourcedRepository<Payment>(new InMemoryEventStore(new InMemoryBus()));
            var requestController = new PaymentRequestsController(eventSourcedRepository);

            var readController = new PaymentReadController(eventSourcedRepository);

            var paymentIdsMapping = new InMemoryPaymentIdsMapping();

            var random = Substitute.For<IRandomnizeAcquiringBankPaymentStatus>();
            random.GeneratePaymentStatus().Returns(paymentStatus);

            var acquiringBank = new AcquiringBankFacade(new AcquiringBankSimulator(random));
            var mediator = new AcquiringBanksMediator(acquiringBank, eventSourcedRepository);

            return new PaymentCQRS(eventSourcedRepository, requestController, readController, paymentIdsMapping,
                acquiringBank, mediator);
        }
    }
}