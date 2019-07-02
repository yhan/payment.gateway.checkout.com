using System;
using SimpleCQRS;

namespace PaymentGateway.Domain.Events
{
    public class PaymentRequested : Event
    {
        public Guid GatewayPaymentId { get; }
        public Guid MerchantId { get; }
        public Guid RequestId { get; }
        public CreditCard CreditCard { get; }
        public Money Amount { get; }

        public PaymentRequested(Guid gatewayPaymentId, Guid merchantId, Guid requestId, CreditCard creditCard,
            Money amount)
        {
            GatewayPaymentId = gatewayPaymentId;
            MerchantId = merchantId;
            RequestId = requestId;
            CreditCard = creditCard;
            Amount = amount;
        }
    }
}