using System;

namespace PaymentGateway.Domain.Events
{
    /// <summary>
    /// Raised when <see cref="PaymentRequested"/> is validated.
    /// The very first event raised on a newly created <see cref="Payment"/>
    /// </summary>
    public class PaymentRequested : Event
    {
        public Guid GatewayPaymentId { get; }
        public Guid MerchantId { get; }
        public Guid RequestId { get; }
        public Card Card { get; }
        public Money Amount { get; }

        public PaymentRequested(Guid gatewayPaymentId, Guid merchantId, Guid requestId, Card card, Money amount)
        {
            GatewayPaymentId = gatewayPaymentId;
            MerchantId = merchantId;
            RequestId = requestId;
            Card = card;
            Amount = amount;
        }
    }
}