using System.Threading.Tasks;
using AcquiringBanks.Stub;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class ReadProjectionsShould
    {
        [Test]
        public async Task Can_Stop_gracefully()
        {
            var bus = NSubstitute.Substitute.For<IPublishEvents>();
            
            var cqrs = await PaymentCQRS.Build(BankPaymentStatus.Accepted, new DefaultBankPaymentIdGenerator(),
                new AlwaysSuccessBankConnectionBehavior(),
                new NoDelayProvider(),
                new NullTimeoutProvider(),
                new NullThrows(),
                bus);


            await cqrs.ReadProjectionsService.StopAsync(default);

            bus.Received(Quantity.Exactly(1)).UnRegisterHandlers();
        }
    }
}