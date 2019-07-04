using System;

namespace PaymentGateway.Domain.Events
{
    /// <summary>
    /// Raised when payment processing failed inside `Gateway`
    /// </summary>
    public class PaymentFaulted : AggregateEvent
    {
        public Guid GatewayPaymentId => this.AggregateId;
        public Guid BankPaymentId { get; }

        public PaymentStatus Status = PaymentStatus.FaultedOnGateway;

        public PaymentFaulted(Guid gatewayPaymentId, Guid bankPaymentId) : base(gatewayPaymentId)
        {
            BankPaymentId = bankPaymentId;
        }
    }
}