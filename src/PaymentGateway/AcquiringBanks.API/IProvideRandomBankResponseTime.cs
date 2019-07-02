using System;

namespace AcquiringBanks.Stub
{
    public interface IProvideRandomBankResponseTime
    {
        TimeSpan Delays();
    }
}