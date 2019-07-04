using System;
using Microsoft.Extensions.Options;
using PaymentGateway.Infrastructure;

namespace PaymentGateway
{
    public class DefaultWaitingBankResponseTimeoutProvider : IProvideTimeout
    {
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;

        public DefaultWaitingBankResponseTimeoutProvider(IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public TimeSpan GetTimeout()
        {
            return TimeSpan.FromMilliseconds(_optionsMonitor.CurrentValue.TimeoutInMilliseconds);
        }
    }
}