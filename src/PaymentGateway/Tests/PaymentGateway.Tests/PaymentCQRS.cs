using AcquiringBanks.API;
using Microsoft.Extensions.Options;
using NSubstitute;
using PaymentGateway.API;
using PaymentGateway.API.ReadAPI;
using PaymentGateway.API.WriteAPI;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using PaymentGateway.Infrastructure.ReadProjector;

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

        internal static PaymentCQRS Build(BankPaymentStatus paymentStatus, SimulateException exceptionSimulator =  null)
        {
            var bus = new InMemoryBus();
            var eventSourcedRepository = new EventSourcedRepository<Payment>(new InMemoryEventStore(bus));
            
            var appSettingsAccessor = Substitute.For<IOptionsMonitor<AppSettings>>();
            appSettingsAccessor.CurrentValue.Returns(new AppSettings() {Executor = ExecutorType.Tests});

            var requestController = new PaymentRequestsController(eventSourcedRepository, appSettingsAccessor);

            var readController = new PaymentReadController(eventSourcedRepository);

            var paymentIdsMapping = new InMemoryPaymentIdsMapping();

            var random = Substitute.For<IRandomnizeAcquiringBankPaymentStatus>();
            random.GeneratePaymentStatus().Returns(paymentStatus);

            var mapIdsFromAcquiringBankToPaymentGateway = new PaymentIdsMemory();
            var acquiringBank = new AcquiringBankFacade(new AcquiringBankSimulator(random), mapIdsFromAcquiringBankToPaymentGateway);
            var mediator = new PaymentProcessor(acquiringBank, eventSourcedRepository, exceptionSimulator);

            var paymentDetailsRepository = new PaymentDetailsRepository();
            var paymentDetailsReadController = new PaymentsDetailsController(mapIdsFromAcquiringBankToPaymentGateway, paymentDetailsRepository);

            var projector = new PaymentReadProjector(bus, paymentDetailsRepository);
            projector.SubscribeToEventsForUpdatingReadModel();

            return new PaymentCQRS(eventSourcedRepository, requestController, readController, paymentDetailsReadController, paymentIdsMapping,
                acquiringBank, mediator);
        }

       
    }
}