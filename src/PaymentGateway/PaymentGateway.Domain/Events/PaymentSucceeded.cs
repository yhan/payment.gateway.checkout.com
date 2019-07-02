using System;

namespace PaymentGateway.Domain.Events
{
    /// <summary>
    /// Raised when acquiring bank accepts payment
    /// </summary>
    public class PaymentSucceeded : Event
    {
        public PaymentStatus Status = PaymentStatus.Success;

        public PaymentSucceeded(Guid gatewayPaymentId, Guid bankPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
            BankPaymentId = bankPaymentId;
        }

        public Guid GatewayPaymentId { get; }
        public Guid BankPaymentId { get; }
    }
}