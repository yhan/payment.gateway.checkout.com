using System;

namespace PaymentGateway.Domain
{
    public class PaymentDetails
    {
        public GatewayPaymentId GatewayPaymentId { get; }
        public AcquiringBankPaymentId BankPaymentId { get; private set; }
        public string CreditCardHolderName { get; }
        public string CreditCardNumber { get; }
        public string CreditCardExpiry { get; }
        public string CreditCardCvv { get; }
        public PaymentStatus Status { get; set; }


        public PaymentDetails(GatewayPaymentId gatewayPaymentId, string creditCardHolderName, string creditCardNumber, string creditCardExpiry, string creditCardCvv)
        {
            GatewayPaymentId = gatewayPaymentId;
            CreditCardHolderName = creditCardHolderName;
            CreditCardNumber = creditCardNumber;
            CreditCardExpiry = creditCardExpiry;
            CreditCardCvv = creditCardCvv;

            Status = PaymentStatus.Requested;
        }


        public void Update(AcquiringBankPaymentId bankPaymentId, PaymentStatus paymentStatus)
        {
            BankPaymentId = bankPaymentId;
            Status = paymentStatus;
        }
    }
}