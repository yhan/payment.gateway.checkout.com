using System;
using System.Text.RegularExpressions;
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

        public bool IsValid(out string invalidReason)
        {
            invalidReason = null;
            if (!CardNumberValid())
            {
                invalidReason = "Invalid card number";
                return false;
            }


            if (!CardCvvValid())
            {
                invalidReason = "Invalid card CVV";
                return false;
            }


            if (!CardExpiryValid())
            {
                invalidReason = "Invalid card expiry";
                return false;
            }

            return true;
        }

        public bool CardCvvValid()
        {
            var reg = "^[0-9]{3}$";
            return !string.IsNullOrWhiteSpace(Cvv) && Regex.IsMatch(Cvv, reg);
        }

        public  bool CardNumberValid()
        {
            var reg = "^[0-9]{4} [0-9]{4} [0-9]{4} [0-9]{4}$";
            return !string.IsNullOrWhiteSpace(Number) && Regex.IsMatch(Number, reg);
        }

        public bool CardExpiryValid()
        {
            var reg = "^(0?[1-9]|1[012])/[0-9]{2}$";
            return !string.IsNullOrWhiteSpace(Expiry) &&  Regex.IsMatch(Expiry, reg);
        }
    }
}