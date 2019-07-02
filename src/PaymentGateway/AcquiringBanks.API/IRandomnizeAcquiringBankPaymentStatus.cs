using System;

namespace AcquiringBanks.Stub
{
    public interface IRandomnizeAcquiringBankPaymentStatus
    {
        BankPaymentStatus GeneratePaymentStatus();
    }

    public class AcquiringBankPaymentStatusRandomnizer : IRandomnizeAcquiringBankPaymentStatus
    {
        private static readonly Random Random = new Random(42);

        public BankPaymentStatus GeneratePaymentStatus()
        {
            var next = Random.Next(0, 2);
            return (BankPaymentStatus) next;
        }
    }
}