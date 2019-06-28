using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class ReadPaymentShould
    {
        [Test]
        public async Task Return_NotFound_When_Payment_does_not_exist()
        {
            Check.That(false).IsTrue();
        }
    }
}