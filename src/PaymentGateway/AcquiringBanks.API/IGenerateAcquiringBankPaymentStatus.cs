using System;

namespace AcquiringBanks.Stub
{
    /// <summary>
    /// Provide random bank payment status
    /// </summary>
    public interface IGenerateAcquiringBankPaymentStatus
    {
        BankPaymentStatus GeneratePaymentStatus();
    }

    /// <summary>
    /// Provide random bank payment status
    /// </summary>
    public class AcquiringBankPaymentStatusRandomnizer : IGenerateAcquiringBankPaymentStatus
    {
        private static readonly Random Random = new Random(42);

        public BankPaymentStatus GeneratePaymentStatus()
        {
            var next = Random.Next(0, 2);
            return (BankPaymentStatus) next;
        }
    }
}