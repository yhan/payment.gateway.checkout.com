using System;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class RandomDelayProviderShould
    {
        [Test]
        public void May_not_timeout()
        {
            IOptionsMonitor<AppSettings> optionsMonitor =  Substitute.For<IOptionsMonitor<AppSettings>>();
            optionsMonitor.CurrentValue.Returns(new AppSettings
            {
                MaxBankLatencyInMilliseconds = 4,
                TimeoutInMilliseconds = 2
            });

            var tolerance = TimeSpan.FromMilliseconds(optionsMonitor.CurrentValue.TimeoutInMilliseconds);

            bool canDelayLessThanTolerance = false;
            bool canDelayMoreThanTolerance = false;

            for (int i = 0; i < 100; i++)
            {
                var delayProvider = new RandomDelayProvider(NullLogger<RandomDelayProvider>.Instance, optionsMonitor);

                if (delayProvider.Delays() > tolerance)
                {
                    canDelayMoreThanTolerance = true;
                }

                if (delayProvider.Delays() < tolerance)
                {
                    canDelayLessThanTolerance = true;
                }
            }

            Check.That(canDelayLessThanTolerance).IsTrue();
            Check.That(canDelayMoreThanTolerance).IsTrue();
        }
    }
}