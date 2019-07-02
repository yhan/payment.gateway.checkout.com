using System;

namespace PaymentGateway.Domain.AcquiringBank
{
    public class PayingAttempt
    {
        public Guid GatewayPaymentId { get;  }


        public string CreditCardNumber { get; }
        public string CreditCardCvv { get; }
        public string CreditCardExpiry { get; }
        public string CreditCardHolderName { get; }

        public double Amount { get;  }

        public string Currency { get;  }
        public Guid MerchantId { get; private set; }

        public PayingAttempt(Guid gatewayPaymentId, Guid merchantId,  string creditCardNumber, string creditCardCvv, string creditCardExpiry, string creditCardHolderName, double amount, string currency)
        {
            GatewayPaymentId = gatewayPaymentId;
            MerchantId = merchantId;
            CreditCardNumber = creditCardNumber;
            CreditCardCvv = creditCardCvv;
            CreditCardExpiry = creditCardExpiry;
            CreditCardHolderName = creditCardHolderName;
            Amount = amount;
            Currency = currency;
        }
    }

    public enum BankPaymentStatus
    {
        Accepted, Rejected
    }
}