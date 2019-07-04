using System;

namespace PaymentGateway.Domain.Events
{
    /// <summary>
    ///     Raised when acquiring bank rejects payment
    /// </summary>
    public class PaymentRejectedByBank : AggregateEvent
    {
        public PaymentStatus Status = PaymentStatus.RejectedByBank;

        public PaymentRejectedByBank(Guid gatewayPaymentId, Guid bankPaymentId) : base(gatewayPaymentId)
        {
            BankPaymentId = bankPaymentId;
        }

        public Guid GatewayPaymentId => AggregateId;
        public Guid BankPaymentId { get; }
    }
}