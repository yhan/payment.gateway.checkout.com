using System;
using SimpleCQRS;

namespace PaymentGateway.Domain.Events
{
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