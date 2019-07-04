using System;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PaymentGateway
{
    /// <inheritdoc />
    /// <summary>
    /// Provides random bank response time
    /// </summary>
    public class RandomDelayProvider : IProvideBankResponseTime
    {
        private readonly ILogger<RandomDelayProvider> _logger;
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;
        private static readonly Random Random = new Random(42);
        private bool _alreadyFailedOnce = false;
        private readonly TimeSpan _alwaysTimeoutDelay;

        public RandomDelayProvider(ILogger<RandomDelayProvider> logger, IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;

            _alwaysTimeoutDelay = TimeSpan.FromSeconds(_optionsMonitor.CurrentValue.MaxBankLatencyInMilliseconds);
        }

        public TimeSpan Delays()
        {
            var tolerance = TimeSpan.FromMilliseconds(_optionsMonitor.CurrentValue.TimeoutInMilliseconds);

            TimeSpan delay;
            if (_alreadyFailedOnce)
            {
                delay = _alwaysTimeoutDelay;
            }
            else
            {
                delay = TimeSpan.FromMilliseconds(Random.Next(0, _optionsMonitor.CurrentValue.MaxBankLatencyInMilliseconds ))
                                .Add(TimeSpan.FromMilliseconds(1)); // to ensure can timeout
            }

            if (delay > tolerance)
            {
                _alreadyFailedOnce = true;
            }
            
            _logger.LogInformation($"Bank response time: {delay}");

            return delay;
        }
    }
}