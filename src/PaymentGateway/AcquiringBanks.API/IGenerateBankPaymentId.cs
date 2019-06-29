using System;

namespace AcquiringBanks.API
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