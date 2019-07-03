using System;

namespace AcquiringBanks.Stub
{
    /// <summary>
    /// Generate acquiring bank's payment unique identifier
    /// </summary>
    public interface IGenerateBankPaymentId
    {
        Guid Generate();
    }

    /// <inheritdoc cref="IGenerateBankPaymentId"/>
    public class DefaultBankPaymentIdGenerator : IGenerateBankPaymentId
    {
        public Guid Generate()
        {
            return Guid.NewGuid();
        }
    }
}