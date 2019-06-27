using System;

namespace PaymentGateway.Domain
{
    public class PaymentRequest
    {
        public Guid RequestId { get; }
        public string CardHolderName { get; }
        public string CardNumber { get; }
        public string Expiry { get; }
        public Money Amount { get; }
        public string Cvv { get; }

        public PaymentRequest(Guid requestId, string cardHolderName, string cardNumber, string expiry, Money amount,
            string cvv)
        {
            RequestId = requestId;
            CardHolderName = cardHolderName;
            CardNumber = cardNumber;
            Expiry = expiry;
            Amount = amount;
            Cvv = cvv;
        }
    }
}