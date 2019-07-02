using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentDetailsDto
    {
        public PaymentGateway.Domain.PaymentStatus Status { get; }
        public Guid? AcquiringBankPaymentId { get; set; }
        public Card Card { get; }
        public bool Approved => Status == PaymentStatus.Success;

        public PaymentDetailsDto(Guid? acquiringBankPaymentId, Card card, PaymentStatus status)
        {
            AcquiringBankPaymentId = acquiringBankPaymentId;
            Card = card;
            Status = status;
        }
    }
}