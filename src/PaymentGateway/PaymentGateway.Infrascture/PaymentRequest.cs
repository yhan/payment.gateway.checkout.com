using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentRequest
    {
        public Guid RequestId { get; }
        public Money Amount { get; }
        public Card Card { get; }
        public Guid MerchantId { get; }

        public PaymentRequest(Guid requestId, Guid merchantId, Money amount, Card card)
        {
            RequestId = requestId;
            MerchantId = merchantId;
            Amount = amount;
            Card = card;
        }
    }

    public class Card
    {
        public string Number { get; }
        public string Expiry { get; }
        public string Cvv { get; }

        public Card(string number, string expiry,  string cvv)
        {
            Number = number;
            Expiry = expiry;
            Cvv = cvv;
        }
    }
}