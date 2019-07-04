using System;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PaymentGateway
{
    /// <summary>
    /// Provides random bank response time
    /// </summary>
    public class RandomDelayProvider : IProvideBankResponseTime
    {
        private readonly ILogger<RandomDelayProvider> _logger;
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;
        private static readonly Random Random = new Random(42);

        public RandomDelayProvider(ILogger<RandomDelayProvider> logger, IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
        }

        public TimeSpan Delays()
        {
            var delay = TimeSpan.FromMilliseconds(Random.Next(0, _optionsMonitor.CurrentValue.MaxBankLatencyInMilliseconds ));
            _logger.LogInformation($"Bank response time: {delay}");

            return delay;
        }
    }
}