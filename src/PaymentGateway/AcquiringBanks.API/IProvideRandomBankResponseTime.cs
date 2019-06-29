using System;

namespace AcquiringBanks.API
{
    public interface IProvideRandomBankResponseTime
    {
        TimeSpan Delays();
    }

    public class DefaultDelayProvider : IProvideRandomBankResponseTime
    {
        private static readonly Random _random = new Random(42);

        public TimeSpan Delays()
        {
            return TimeSpan.FromSeconds(_random.Next(0, 11));
        }
    }
}