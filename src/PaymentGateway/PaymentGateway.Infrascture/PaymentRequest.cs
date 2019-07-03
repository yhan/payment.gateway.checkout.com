using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentRequest:  IValidatableObject
    {
        [Required]
        public Guid RequestId { get; }

        [Required]
        public Money Amount { get; }

        [Required]
        public Card Card { get; }

        [Required]
        public Guid MerchantId { get; }

        public PaymentRequest(Guid requestId, Guid merchantId, Money amount, Card card)
        {
            RequestId = requestId;
            MerchantId = merchantId;
            Amount = amount;
            Card = card;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (IsEmpty(RequestId))
            {
                results.Add(new ValidationResult("Request id missing"));
            }

            if (IsEmpty(MerchantId))
            {
                results.Add(new ValidationResult("Merchant id missing"));
            }

            if (!Card.IsValid(out var invalids))
            {
                results.AddRange(invalids.Select(x => new ValidationResult(x)));
            }

            if (!Amount.IsValid(out List<string> amountInvalids))
            {
                results.AddRange(amountInvalids.Select(x => new ValidationResult(x)));
            }

            return results;
        }

        private static bool IsEmpty(Guid id)
        {
            return id == Guid.Empty;
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

        public bool IsValid(out List<string> invalids)
        {
            invalids = new List<string>();

            if (!CardNumberValid())
            {
                invalids.Add("Invalid card number");
            }


            if (!CardCvvValid())
            {
                invalids.Add("Invalid card CVV");
            }


            if (!CardExpiryValid())
            {
                invalids.Add("Invalid card expiry");
            }

            return !invalids.Any();
        }

        private bool CardCvvValid()
        {
            var reg = "^[0-9]{3}$";
            return !string.IsNullOrWhiteSpace(Cvv) && Regex.IsMatch(Cvv, reg);
        }

        private bool CardNumberValid()
        {
            var reg = "^[0-9]{4} [0-9]{4} [0-9]{4} [0-9]{4}$";
            return !string.IsNullOrWhiteSpace(Number) && Regex.IsMatch(Number, reg);
        }

        private bool CardExpiryValid()
        {
            var reg = "^(0?[1-9]|1[012])/[0-9]{2}$";
            return !string.IsNullOrWhiteSpace(Expiry) &&  Regex.IsMatch(Expiry, reg);
        }
    }
}