using System;

namespace PaymentGateway.Domain.Events
{
    /// <summary>
    /// Raised when payment processing failed inside `Gateway`
    /// </summary>
    public class PaymentFaulted : Event
    {
        public Guid GatewayPaymentId { get; }
        public Guid BankPaymentId { get; }

        public PaymentStatus Status = PaymentStatus.FaultedOnGateway;

        public PaymentFaulted(Guid gatewayPaymentId, Guid bankPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
            BankPaymentId = bankPaymentId;
        }
    }
}