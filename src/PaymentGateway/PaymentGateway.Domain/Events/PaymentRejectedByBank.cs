using System;

namespace PaymentGateway.Domain.Events
{
    /// <summary>
    /// Raised when acquiring bank rejects payment
    /// </summary>
    public class PaymentRejectedByBank : Event
    {
        public PaymentStatus Status = PaymentStatus.RejectedByBank;

        public PaymentRejectedByBank(Guid gatewayPaymentId, Guid bankPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
            BankPaymentId = bankPaymentId;
        }

        public Guid GatewayPaymentId { get; }
        public Guid BankPaymentId { get; }
    }
}