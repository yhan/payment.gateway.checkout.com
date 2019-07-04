using System;
using PaymentGateway.Domain.Events;

namespace PaymentGateway.Domain
{
    public class PaymentTimeoutOccurred : AggregateEvent
    {
        public Guid GatewayPaymentId => AggregateId;

        public PaymentStatus Status = PaymentStatus.Timeout;

        public PaymentTimeoutOccurred(Guid gatewayPaymentId): base(gatewayPaymentId)
        {
        }
    }
}