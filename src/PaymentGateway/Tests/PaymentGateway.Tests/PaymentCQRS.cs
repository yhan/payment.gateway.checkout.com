using System;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBanks.API;
using Microsoft.Extensions.Options;
using NSubstitute;
using PaymentGateway.API;
using PaymentGateway.API.ReadAPI;
using PaymentGateway.API.ReadProjector;
using PaymentGateway.API.WriteAPI;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    internal class PaymentCQRS
    {
        internal PaymentsDetailsController PaymentDetailsReadController { get; }
        internal PaymentProcessor PaymentProcessor{ get; }
        internal InMemoryPaymentIdsMapping PaymentIdsMapping{ get; }
        internal PaymentReadController ReadController{ get; }
        internal PaymentRequestsController RequestsController{ get; }

        private PaymentCQRS(PaymentRequestsController requestController, PaymentReadController readController,
            PaymentsDetailsController paymentDetailsReadController, InMemoryPaymentIdsMapping paymentIdsMapping, PaymentProcessor paymentProcessor)
        {
            PaymentDetailsReadController = paymentDetailsReadController;
            RequestsController = requestController;
            ReadController = readController;
            PaymentIdsMapping = paymentIdsMapping;
            PaymentProcessor = paymentProcessor;
        }

        internal static async Task<PaymentCQRS> Build(BankPaymentStatus paymentStatus,
            IGenerateBankPaymentId bankPaymentIdGenerator, SimulateException exceptionSimulator = null)
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
            var acquiringBank = new AcquiringBankFacade(new AcquiringBankSimulator(random, bankPaymentIdGenerator, new DelayProvider()), mapIdsFromAcquiringBankToPaymentGateway);
            var mediator = new PaymentProcessor(acquiringBank, eventSourcedRepository, exceptionSimulator);

            var paymentDetailsRepository = new PaymentDetailsRepository();
            var paymentDetailsReadController = new PaymentsDetailsController(mapIdsFromAcquiringBankToPaymentGateway, paymentDetailsRepository);

            var readProjections = new ReadProjections(bus, paymentDetailsRepository);
            await readProjections.StartAsync(new CancellationToken(false));

            return new PaymentCQRS(requestController, readController, paymentDetailsReadController, paymentIdsMapping, mediator);
        }
    }

    internal class DelayProvider : IProvideRandomBankResponseTime
    {
        public TimeSpan Delays()
        {
            return TimeSpan.FromMilliseconds(1);
        }
    }
}