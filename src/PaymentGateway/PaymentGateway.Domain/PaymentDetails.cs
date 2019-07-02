namespace PaymentGateway.Domain
{
    public class PaymentDetails
    {
        public GatewayPaymentId GatewayPaymentId { get; }
        public AcquiringBankPaymentId BankPaymentId { get; private set; }
        public string CardNumber { get; }
        public string CardExpiry { get; }
        public string CardCvv { get; }
        public PaymentStatus Status { get; set; }


        public PaymentDetails(GatewayPaymentId gatewayPaymentId, string cardNumber, string cardExpiry, string cardCvv)
        {
            GatewayPaymentId = gatewayPaymentId;
            CardNumber = cardNumber;
            CardExpiry = cardExpiry;
            CardCvv = cardCvv;

            Status = PaymentStatus.Requested;
        }


        public void Update(AcquiringBankPaymentId bankPaymentId, PaymentStatus paymentStatus)
        {
            BankPaymentId = bankPaymentId;
            Status = paymentStatus;
        }
    }
}