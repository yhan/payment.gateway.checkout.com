using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentDetailsDto
    {
        public string CreditCardNumber { get; }
        public string CreditCardHolderName { get; }
        public string CreditCardExpiry { get; }
        public string CreditCardCvv { get; }
        public PaymentGateway.Domain.PaymentStatus Status { get; }
        public Guid AcquiringBankPaymentId { get; set; }


        public PaymentDetailsDto(Guid acquiringBankPaymentId, string creditCardNumber,
            string creditCardHolderName, string creditCardExpiry, string creditCardCvv, PaymentStatus status)
        {
            AcquiringBankPaymentId = acquiringBankPaymentId;

            CreditCardNumber = creditCardNumber;
            CreditCardHolderName = creditCardHolderName;
            CreditCardExpiry = creditCardExpiry;
            CreditCardCvv = creditCardCvv;
            Status = status;
        }
    }
}