using System;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using PaymentGateway.ReadProjector;
using PaymentGateway.WriteAPI;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using PaymentGateway.ReadAPI;

namespace PaymentGateway.Tests
{
    internal class PaymentCQRS
    {
        internal PaymentsDetailsController PaymentDetailsReadController { get; }
        internal PaymentProcessor PaymentProcessor{ get; }
        public GatewayPaymentsIdsController GatewayPaymentsIdsController { get; }
        internal PaymentRequestsMemory PaymentRequestsMemory{ get; }
        internal PaymentReadController PaymentReadController{ get; }
        internal PaymentRequestsController RequestsController{ get; }
        public AcquiringBankPaymentsIdsController AcquiringBankPaymentsIdsController { get; set; }

        private PaymentCQRS(PaymentRequestsController requestController, PaymentReadController paymentReadController,
            PaymentsDetailsController paymentDetailsReadController, PaymentRequestsMemory paymentRequestsMemory,
            PaymentProcessor paymentProcessor, GatewayPaymentsIdsController gatewayGatewayPaymentsIdsController,
            AcquiringBankPaymentsIdsController acquiringBankPaymentsIdsController)
        {
            PaymentDetailsReadController = paymentDetailsReadController;
            RequestsController = requestController;
            PaymentReadController = paymentReadController;
            PaymentRequestsMemory = paymentRequestsMemory;
            PaymentProcessor = paymentProcessor;
            GatewayPaymentsIdsController = gatewayGatewayPaymentsIdsController;
            AcquiringBankPaymentsIdsController = acquiringBankPaymentsIdsController;
        }

        internal static async Task<PaymentCQRS> Build(BankPaymentStatus paymentStatus,
            IGenerateBankPaymentId bankPaymentIdGenerator, 
            IConnectToAcquiringBanks bankConnectionBehavior,  
            IProvideBankResponseTime delayProvider,
            IProvideTimeout providerForBankResponseWaiting,
            SimulateGatewayException gatewayExceptionSimulator = null)
        {
            var bus = new InMemoryBus();
            var eventSourcedRepository = new EventSourcedRepository<Payment>(new InMemoryEventStore(bus));
            
            var appSettingsAccessor = Substitute.For<IOptionsMonitor<AppSettings>>();
            appSettingsAccessor.CurrentValue.Returns(new AppSettings {Executor = ExecutorType.Tests});

            var random = Substitute.For<IGenerateAcquiringBankPaymentStatus>();
            random.GeneratePaymentStatus().Returns(paymentStatus);

            var paymentsIdsMemory = new PaymentIdsMemory();
            var bankAdapterSelector = new BankAdapterSelector(random, bankPaymentIdGenerator, delayProvider, bankConnectionBehavior, paymentsIdsMemory, NullLogger<BankAdapterSelector>.Instance);
            var merchantToBankAdapterMapper = new MerchantToBankAdapterMapper(bankAdapterSelector);
            var paymentRequestsMemory = new PaymentRequestsMemory();
            var mediator = new PaymentProcessor(eventSourcedRepository, NullLogger<PaymentProcessor>.Instance, providerForBankResponseWaiting, gatewayExceptionSimulator);
            var optionMonitor = Substitute.For<IOptionsMonitor<AppSettings>>();
            optionMonitor.CurrentValue.Returns(new AppSettings
            {
                Executor = ExecutorType.Tests
            });

            var paymentRequestCommandHandler = new PaymentRequestCommandHandler(eventSourcedRepository, paymentRequestsMemory, mediator, merchantToBankAdapterMapper, new RequestBankSynchronyMaster(optionMonitor), NullLogger<PaymentRequestCommandHandler>.Instance);
            var requestController = new PaymentRequestsController(paymentRequestCommandHandler , NullLogger<PaymentRequestsController>.Instance);

            var readController = new PaymentReadController(eventSourcedRepository);
            
            var paymentDetailsRepository = new PaymentDetailsRepository();
            var paymentDetailsReadController = new PaymentsDetailsController(paymentsIdsMemory, paymentDetailsRepository);

            var readProjections = new ReadProjections(bus, paymentDetailsRepository);
            await readProjections.StartAsync(new CancellationToken(false));

            var gatewayPaymentsIdsController = new GatewayPaymentsIdsController(paymentsIdsMemory);
            var acquiringBankPaymentsIdsController = new AcquiringBankPaymentsIdsController(paymentsIdsMemory);

            return new PaymentCQRS(requestController, readController, paymentDetailsReadController, paymentRequestsMemory, mediator, gatewayPaymentsIdsController, acquiringBankPaymentsIdsController);
        }

        public static IProvideTimeout TimeoutProviderForBankResponseWaiting(TimeSpan timeoutTolerance)
        {
            var timeoutProviderForBankResponseWaiting = Substitute.For<IProvideTimeout>();
            timeoutProviderForBankResponseWaiting.GetTimeout().Returns(timeoutTolerance);
            return timeoutProviderForBankResponseWaiting;
        }
    }

    internal class DelayProviderForTesting : IProvideBankResponseTime
    {
        private readonly TimeSpan _delay;

        public DelayProviderForTesting(TimeSpan delay)
        {
            _delay = delay;
        }

        public TimeSpan Delays()
        {
            return _delay;
        }
    }
}