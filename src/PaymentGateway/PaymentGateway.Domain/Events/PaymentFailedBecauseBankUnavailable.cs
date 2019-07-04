using System;

namespace PaymentGateway.Domain.Events
{
    public class PaymentFailedBecauseBankUnavailable : AggregateEvent
    {
        public Guid GatewayPaymentId { get; }

        public PaymentStatus Status = PaymentStatus.BankUnavailable;

        public PaymentFailedBecauseBankUnavailable(Guid gatewayPaymentId) : base(gatewayPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
        }
    }
}