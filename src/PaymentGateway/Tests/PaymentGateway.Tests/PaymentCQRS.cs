using AcquiringBanks.API;
using NSubstitute;
using PaymentGateway.API.ReadAPI;
using PaymentGateway.API.WriteAPI;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    internal class PaymentCQRS
    {
        internal PaymentsDetailsController PaymentDetailsReadController { get; }
        internal AcquiringBankFacade AcquiringBank{ get; }
        internal EventSourcedRepository<Payment> EventSourcedRepository{ get; }
        internal PaymentProcessor PaymentProcessor{ get; }
        internal InMemoryPaymentIdsMapping PaymentIdsMapping{ get; }
        internal PaymentReadController ReadController{ get; }
        internal PaymentRequestsController RequestController{ get; }

        private PaymentCQRS(EventSourcedRepository<Payment> eventSourcedRepository,
            PaymentRequestsController requestController, PaymentReadController readController,
            PaymentsDetailsController paymentDetailsReadController, InMemoryPaymentIdsMapping paymentIdsMapping,
            AcquiringBankFacade acquiringBank, PaymentProcessor paymentProcessor)
        {
            PaymentDetailsReadController = paymentDetailsReadController;
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

            var acquiringBank = new AcquiringBankFacade(new AcquiringBankSimulator(random), TODO);
            var mediator = new PaymentProcessor(acquiringBank, eventSourcedRepository);

            

            var paymentDetailsReadController = new PaymentsDetailsController(new BankToGatewayMapper(), new PaymentDetailsRepository());

            return new PaymentCQRS(eventSourcedRepository, requestController, readController, paymentDetailsReadController, paymentIdsMapping,
                acquiringBank, mediator);
        }
    }
}