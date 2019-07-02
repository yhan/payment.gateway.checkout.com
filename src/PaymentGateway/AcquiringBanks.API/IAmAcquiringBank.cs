using System;

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
    }

    public interface IBankResponse
    {
    }
}