using System;
using PaymentGateway.Domain.Events;

namespace PaymentGateway.Domain
{
    /// <summary>
    ///     Raised when bank too much time to respond or never responded
    /// </summary>
    public class PaymentTimeoutOccurred : AggregateEvent
    {
        public Guid GatewayPaymentId => AggregateId;

        public PaymentStatus Status = PaymentStatus.Timeout;

        public PaymentTimeoutOccurred(Guid gatewayPaymentId): base(gatewayPaymentId)
        {
        }
    }
}