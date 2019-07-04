using System;
using Microsoft.Extensions.Options;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class DefaultTimeoutProviderWaitingBankResponseShould
    {
        [Test]
        public void Get_timeout_from_AppSettings_configuration_section()
        {
            var optionsMonitor = NSubstitute.Substitute.For<IOptionsMonitor<AppSettings>>();
            optionsMonitor.CurrentValue.Returns(new AppSettings()
            {
                TimeoutInMilliseconds = 32
            });

            var timeoutProvider = new DefaultWaitingBankResponseTimeoutProvider(optionsMonitor);
            var timeout = timeoutProvider.GetTimeout();
            Check.That(timeout).IsEqualTo(TimeSpan.FromMilliseconds(32));
        }
    }
}