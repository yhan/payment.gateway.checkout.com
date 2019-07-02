using System;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBanks.API;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        public GatewayPaymentsIdsController GatewayPaymentsIdsController { get; }
        internal InMemoryPaymentRequests PaymentRequests{ get; }
        internal PaymentReadController PaymentReadController{ get; }
        internal PaymentRequestsController RequestsController{ get; }
        public AcquiringBankPaymentsIdsController AcquiringBankPaymentsIdsController { get; set; }

        private PaymentCQRS(PaymentRequestsController requestController, PaymentReadController paymentReadController,
            PaymentsDetailsController paymentDetailsReadController, InMemoryPaymentRequests paymentRequests,
            PaymentProcessor paymentProcessor, GatewayPaymentsIdsController gatewayGatewayPaymentsIdsController,
            AcquiringBankPaymentsIdsController acquiringBankPaymentsIdsController)
        {
            PaymentDetailsReadController = paymentDetailsReadController;
            RequestsController = requestController;
            PaymentReadController = paymentReadController;
            PaymentRequests = paymentRequests;
            PaymentProcessor = paymentProcessor;
            GatewayPaymentsIdsController = gatewayGatewayPaymentsIdsController;
            AcquiringBankPaymentsIdsController = acquiringBankPaymentsIdsController;
        }

        internal static async Task<PaymentCQRS> Build(BankPaymentStatus paymentStatus,
            IGenerateBankPaymentId bankPaymentIdGenerator, 
            IConnectToAcquiringBanks bankConnectionBehavior,  
            SimulateGatewayException gatewayExceptionSimulator = null)
        {
            var bus = new InMemoryBus();
            var eventSourcedRepository = new EventSourcedRepository<Payment>(new InMemoryEventStore(bus));
            
            var appSettingsAccessor = Substitute.For<IOptionsMonitor<AppSettings>>();
            appSettingsAccessor.CurrentValue.Returns(new AppSettings {Executor = ExecutorType.Tests});

            var requestController = new PaymentRequestsController(eventSourcedRepository, appSettingsAccessor, NullLogger<PaymentRequestsController>.Instance);

            var readController = new PaymentReadController(eventSourcedRepository);

            var paymentIdsMapping = new InMemoryPaymentRequests();

            var random = Substitute.For<IRandomnizeAcquiringBankPaymentStatus>();
            random.GeneratePaymentStatus().Returns(paymentStatus);

            var paymentsIdsMemory = new PaymentIdsMemory();


            var mediator = new PaymentProcessor(new MerchantToBankAdapterMapper(new BankAdapterSelector(random, bankPaymentIdGenerator, new DelayProvider(), bankConnectionBehavior, paymentsIdsMemory, NullLogger<BankAdapterSelector>.Instance)), eventSourcedRepository, gatewayExceptionSimulator);

            var paymentDetailsRepository = new PaymentDetailsRepository();
            var paymentDetailsReadController = new PaymentsDetailsController(paymentsIdsMemory, paymentDetailsRepository);

            var readProjections = new ReadProjections(bus, paymentDetailsRepository);
            await readProjections.StartAsync(new CancellationToken(false));

            var gatewayPaymentsIdsController = new GatewayPaymentsIdsController(paymentsIdsMemory);
            var acquiringBankPaymentsIdsController = new AcquiringBankPaymentsIdsController(paymentsIdsMemory);

            return new PaymentCQRS(requestController, readController, paymentDetailsReadController, paymentIdsMapping, mediator, gatewayPaymentsIdsController, acquiringBankPaymentsIdsController);
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