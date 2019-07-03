using System;

namespace AcquiringBanks.Stub
{
    /// <summary>
    /// Mimic bank response time
    /// </summary>
    public interface IProvideBankResponseTime
    {
        TimeSpan Delays();
    }
}