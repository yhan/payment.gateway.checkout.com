﻿namespace PaymentGateway.Domain
{
    public class PaymentDetails
    {
        public PaymentDetails(Card card)
        {
            Card = card;
        }

        public Card Card { get; }

        public AcquiringBankPaymentId? BankPaymentId { get; private set; }
        
        public PaymentStatus Status { get; set; }

        public void Update(AcquiringBankPaymentId bankPaymentId, PaymentStatus paymentStatus)
        {
            BankPaymentId = bankPaymentId;
            Status = paymentStatus;
        }

        public void Update(PaymentStatus paymentStatus)
        {
            Status = paymentStatus;
        }
    }
}