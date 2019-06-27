using System;

namespace PaymentGateway.Domain
{
    public class PaymentRequest
    {
        public Guid RequestId { get; }
        public string FullName { get; }
        public string CardNumber { get; }
        public string Expiry { get; }
        public Money Money { get; }
        public string Cvv { get; }

        public PaymentRequest(Guid requestId, string fullName, string cardNumber, string expiry, Money money,
            string cvv)
        {
            RequestId = requestId;
            FullName = fullName;
            CardNumber = cardNumber;
            Expiry = expiry;
            Money = money;
            Cvv = cvv;
        }
    }
}