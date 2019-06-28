using System.Threading.Tasks;
using AcquiringBanks.API;
using NFluent;
using NUnit.Framework;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class ReadPaymentShould
    {
        [Test]
        [Ignore("todo")]
        public async Task Return_NotFound_When_Payment_does_not_exist()
        {
            //var (requestsController, readController, paymentIdsMapping, paymentProcessor, acquiringBank) = PaymentCQRS.Build(AcquiringBanks.API.BankPaymentStatus.Rejected);
        }
    }
}