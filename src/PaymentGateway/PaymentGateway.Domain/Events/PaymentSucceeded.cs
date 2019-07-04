using System;

namespace PaymentGateway.Domain.Events
{
    /// <summary>
    ///     Raised when acquiring bank accepts payment
    /// </summary>
    public class PaymentSucceeded : AggregateEvent
    {
        public PaymentStatus Status = PaymentStatus.Success;

        public PaymentSucceeded(Guid gatewayPaymentId, Guid bankPaymentId) : base(gatewayPaymentId)
        {
            BankPaymentId = bankPaymentId;
        }

        public Guid GatewayPaymentId => AggregateId;
        public Guid BankPaymentId { get; }
    }
}