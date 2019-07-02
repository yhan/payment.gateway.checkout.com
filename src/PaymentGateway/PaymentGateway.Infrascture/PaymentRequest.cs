using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentRequest
    {
        public Guid RequestId { get; }
        public string CardHolderName { get; }
        public string CardNumber { get; }
        public string Expiry { get; }
        public Money Amount { get; }
        public string Cvv { get; }
        public Guid MerchantId { get; }

        public PaymentRequest(Guid requestId, Guid merchantId, string cardHolderName, string cardNumber, string expiry, Money amount,
            string cvv)
        {
            RequestId = requestId;
            MerchantId = merchantId;
            CardHolderName = cardHolderName;
            CardNumber = cardNumber;
            Expiry = expiry;
            Amount = amount;
            Cvv = cvv;
        }
    }
}