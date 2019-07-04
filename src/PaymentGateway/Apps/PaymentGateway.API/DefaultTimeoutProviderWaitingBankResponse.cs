using System;
using Microsoft.Extensions.Options;
using PaymentGateway.Infrastructure;

namespace PaymentGateway
{
    public class DefaultTimeoutProviderWaitingBankResponse : IProvideTimeout
    {
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;

        public DefaultTimeoutProviderWaitingBankResponse(IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public TimeSpan GetTimeout()
        {
            return TimeSpan.FromMilliseconds(_optionsMonitor.CurrentValue.TimeoutInMilliseconds);
        }
    }
}