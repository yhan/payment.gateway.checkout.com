using System;
using System.Threading.Tasks;

namespace AcquiringBanks.API
{
    public class BankResponse : IBankResponse
    {
        public Guid BankPaymentId { get; }
        public Guid GatewayPaymentId { get; }
        public AcquiringBanks.API.BankPaymentStatus PaymentStatus { get; }

        public BankResponse(Guid bankPaymentId, Guid gatewayPaymentId, BankPaymentStatus paymentStatus)
        {
            BankPaymentId = bankPaymentId;
            GatewayPaymentId = gatewayPaymentId;
            PaymentStatus = paymentStatus;
        }

        public bool BankContactable()
        {
            return true;
        }
    }

    public interface IBankResponse
    {
        bool BankContactable();
    }
}