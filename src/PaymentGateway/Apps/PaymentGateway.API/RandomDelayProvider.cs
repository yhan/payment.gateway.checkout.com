using System;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;

namespace PaymentGateway
{
    /// <summary>
    /// Provides random bank response time
    /// </summary>
    public class RandomDelayProvider : IProvideBankResponseTime
    {
        private readonly ILogger<RandomDelayProvider> _logger;
        private static readonly Random Random = new Random(42);

        public RandomDelayProvider(ILogger<RandomDelayProvider> logger)
        {
            _logger = logger;
        }

        public TimeSpan Delays()
        {
            var delay = TimeSpan.FromSeconds(Random.Next(0, 5));
            _logger.LogInformation($"Bank response time: {delay}");

            return delay;
        }
    }
}