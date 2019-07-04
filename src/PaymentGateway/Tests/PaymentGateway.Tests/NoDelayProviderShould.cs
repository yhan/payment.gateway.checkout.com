using System;
using NFluent;
using NUnit.Framework;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class NoDelayProviderShould
    {
        [Test]
        public void Return_no_delay()
        {
            var delayProvider = new NoDelayProvider();
            Check.That(delayProvider.Delays()).IsEqualTo(TimeSpan.Zero);
        }
    }
}