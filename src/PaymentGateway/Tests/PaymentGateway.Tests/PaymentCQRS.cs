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
        internal static (PaymentRequestsController, PaymentReadController, IProvidePaymentIdsMapping, IProcessPayment, AcquiringBankFacade ) Build(AcquiringBanks.API.BankPaymentStatus paymentStatus)
        {
            var eventSourcedRepository = new EventSourcedRepository<Payment>(new InMemoryEventStore(new InMemoryBus()));
            var requestController = new PaymentRequestsController(eventSourcedRepository);

            var readController = new PaymentReadController(eventSourcedRepository);

            var paymentIdsMapping = new InMemoryPaymentIdsMapping();

            var random = Substitute.For<IRandomnizeAcquiringBankPaymentStatus>();
            random.GeneratePaymentStatus().Returns(paymentStatus);

            var acquiringBank = new AcquiringBankFacade(new AcquiringBankSimulator(random));
            var mediator = new AcquiringBanksMediator(acquiringBank, eventSourcedRepository);

            return (requestController, readController, paymentIdsMapping, mediator, acquiringBank);
        }
    }
}