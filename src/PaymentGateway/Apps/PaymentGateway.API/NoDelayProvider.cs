using System;
using AcquiringBanks.Stub;

namespace PaymentGateway
{
    ///<inheritdoc cref="IProvideBankResponseTime"/>
    /// <summary>
    /// Can be used for performance testing: test Gateway internal latency.
    /// </summary>
    internal class NoDelayProvider : IProvideBankResponseTime
    {
        public TimeSpan Delays()
        {
            return TimeSpan.Zero;
        }
    }
}