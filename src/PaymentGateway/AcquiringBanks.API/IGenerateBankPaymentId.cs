using System;

namespace AcquiringBanks.Stub
{
    public interface IGenerateBankPaymentId
    {
        Guid Generate();
    }

    public class DefaultBankPaymentIdGenerator : IGenerateBankPaymentId
    {
        public Guid Generate()
        {
            return Guid.NewGuid();
        }
    }
}