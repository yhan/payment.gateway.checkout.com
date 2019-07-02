using System;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.API
{
    public class DefaultDelayProvider : IProvideRandomBankResponseTime
    {
        private readonly ILogger<DefaultDelayProvider> _logger;
        private static readonly Random _random = new Random(42);

        public DefaultDelayProvider(ILogger<DefaultDelayProvider> logger)
        {
            _logger = logger;
        }

        public TimeSpan Delays()
        {
            var delay = TimeSpan.FromSeconds(_random.Next(0, 5));
            _logger.LogInformation($"Bank response time: {delay}");

            return delay;
        }
    }
}