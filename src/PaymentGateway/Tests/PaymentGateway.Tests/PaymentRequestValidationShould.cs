using System;
using System.Linq;
using NFluent;
using NUnit.Framework;
using PaymentGateway.Domain;

namespace PaymentGateway.Tests
{
    public class PaymentRequestValidationShould
    {

        [Test, Combinatorial]
        public void Detect_malformatted_PaymentRequest(
            [Values("456A 4589 1052 4568", "4560 ???9 1052 4568", "45601199 1052 4568", "45601199 1052 4568 ")]string cardNumber,
            [Values("05/19", "13/12", "112", "aaa", "aa/ba")]string expiry, 
            [Values( "a45", "789456123")]string cvv, 
            [Values( "EU", "USDUSD")]string currency, 
            [Values( -42, 0)]double value)
        {
            var card = new Infrastructure.Card(cardNumber, expiry, cvv);
            var paymentRequest = new PaymentGateway.Infrastructure.PaymentRequest(Guid.Empty, Guid.Empty, amount: new Money(currency, value), card: card);
            var validationResults = paymentRequest.Validate(null);

            Check.That(validationResults.Select(x =>x.ErrorMessage))
                .IsOnlyMadeOf("Request id missing", 
                    "Merchant id missing", 
                    "Invalid card number", 
                    "Invalid card CVV", 
                    "Invalid card expiry",
                    "Amount should be greater than 0", 
                    "Currency is absent or not correctly formatted");
        }

        [Test]
        public void Consider_as_valid_paymentRequest()
        {
            var card = new Infrastructure.Card("4564 4589 1052 4568", "05/22", "123");
            var paymentRequest = new PaymentGateway.Infrastructure.PaymentRequest(Guid.NewGuid(), Guid.NewGuid(), amount: new Money("EUR", 42), card: card);
            var validationResults = paymentRequest.Validate(null);
            Check.That(validationResults).IsEmpty();
        }
    }
}