using System;
using SimpleCQRS;

namespace PaymentGateway.Domain
{
    public class PaymentFaulted : Event
    {
        public Guid GatewayPaymentId { get; }

        public PaymentStatus Status = PaymentStatus.FaultedOnGateway;

        public PaymentFaulted(Guid gatewayPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
        }
    }
}