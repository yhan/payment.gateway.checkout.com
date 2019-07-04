using System;

namespace PaymentGateway.Domain
{
    public class PayingAttempt
    {
        public Guid GatewayPaymentId { get;  }
        public Guid PaymentRequestId { get; }

        public string CardNumber { get; }
        public string CardCvv { get; }
        public string CardExpiry { get; }

        public double Amount { get;  }
        public string Currency { get;  }


        public PayingAttempt(Guid gatewayPaymentId, Guid paymentRequestId, string cardNumber, string cardCvv, string cardExpiry, double amount, string currency)
        {
            GatewayPaymentId = gatewayPaymentId;
            PaymentRequestId = paymentRequestId;
            CardNumber = cardNumber;
            CardCvv = cardCvv;
            CardExpiry = cardExpiry;
            Amount = amount;
            Currency = currency;
        }
    }
}