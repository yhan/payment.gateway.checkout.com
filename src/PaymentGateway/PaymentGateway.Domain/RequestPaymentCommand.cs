using System;
using PaymentGateway.Domain.Commands;

namespace PaymentGateway.Domain
{
    public class RequestPaymentCommand : Command
    {
        public RequestPaymentCommand(Guid gatewayPaymentId, Guid merchantId, Guid requestId, Card card, Money amount)
        {
            GatewayPaymentId = gatewayPaymentId;
            MerchantId = merchantId;
            RequestId = requestId;
            Card = card;
            Amount = amount;
        }

        public Guid GatewayPaymentId { get; }
        public Guid RequestId { get; }
        public Card Card { get; }
        public Money Amount { get; }
        public Guid MerchantId { get; set; }
    }

    public class Card
    {
        public Card(string number, string cvv, string expiry)
        {
            Number = number;
            Cvv = cvv;
            Expiry = expiry;
        }

        public string Number { get; }
        public string Cvv { get; }
        public string Expiry { get; }
    }
}