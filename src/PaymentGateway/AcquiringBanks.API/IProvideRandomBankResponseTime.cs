using System;

namespace AcquiringBanks.API
{
    public interface IProvideRandomBankResponseTime
    {
        TimeSpan Delays();
    }
}