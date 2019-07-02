using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentDetailsDto
    {
        public string CardNumber { get; }
        public string CardExpiry { get; }
        public string CardCvv { get; }
        public PaymentGateway.Domain.PaymentStatus Status { get; }
        public Guid AcquiringBankPaymentId { get; set; }


        public PaymentDetailsDto(Guid acquiringBankPaymentId, string cardNumber, string cardExpiry, string cardCvv, PaymentStatus status)
        {
            AcquiringBankPaymentId = acquiringBankPaymentId;

            CardNumber = cardNumber;
            CardExpiry = cardExpiry;
            CardCvv = cardCvv;
            Status = status;
        }
    }
}