namespace PaymentGateway.Domain
{
    public class PaymentDetails
    {
        public PaymentDetails(GatewayPaymentId gatewayPaymentId, Card card)
        {
            GatewayPaymentId = gatewayPaymentId;
            Card = card;
        }

        public GatewayPaymentId GatewayPaymentId { get; }
        public Card Card { get; }

        public AcquiringBankPaymentId? BankPaymentId { get; private set; }
        public PaymentStatus Status { get; set; }


        public void Update(AcquiringBankPaymentId bankPaymentId, PaymentStatus paymentStatus)
        {
            BankPaymentId = bankPaymentId;
            Status = paymentStatus;
        }
    }
}