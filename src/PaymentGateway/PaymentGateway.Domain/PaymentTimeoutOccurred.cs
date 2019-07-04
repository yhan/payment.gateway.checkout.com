using System;
using PaymentGateway.Domain.Events;

namespace PaymentGateway.Domain
{
    public class PaymentTimeoutOccurred : Event
    {
        public Guid GatewayPaymentId { get; }

        public PaymentStatus Status = PaymentStatus.Timeout;

        public PaymentTimeoutOccurred(Guid gatewayPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
        }
    }
}