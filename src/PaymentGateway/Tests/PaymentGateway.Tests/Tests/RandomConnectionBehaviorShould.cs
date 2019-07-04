using System.Threading.Tasks;
using AcquiringBanks.Stub;
using NFluent;
using NSubstitute;
using NUnit.Framework;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class RandomConnectionBehaviorShould
    {
        [Test]
        public async Task Generate_random_connection_behavior()
        {
            var random = new RandomConnectionBehavior();
            bool connected = false;
            bool canFail = false;
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    connected = await random.Connect();
                }
                catch (FailedConnectionToBankException)
                {
                    canFail = true;
                }
            }

            Check.That(connected).IsTrue();
            Check.That(canFail).IsTrue();
        }
    }
}