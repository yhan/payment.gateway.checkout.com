using System;
using SimpleCQRS;

namespace PaymentGateway.Domain.Events
{
    public class PaymentRequested : Event
    {
        public Guid GatewayPaymentId { get; }
        public Guid MerchantId { get; }
        public Guid RequestId { get; }
        public Card Card { get; }
        public Money Amount { get; }

        public PaymentRequested(Guid gatewayPaymentId, Guid merchantId, Guid requestId, Card card,
            Money amount)
        {
            GatewayPaymentId = gatewayPaymentId;
            MerchantId = merchantId;
            RequestId = requestId;
            Card = card;
            Amount = amount;
        }
    }
}