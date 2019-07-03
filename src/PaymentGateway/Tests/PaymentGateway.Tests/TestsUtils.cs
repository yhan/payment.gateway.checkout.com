using System;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    public static class TestsUtils
    {
        public static PaymentRequest BuildPaymentRequest(Guid requestId, Guid merchantId)
        {
            return new PaymentRequest(requestId, merchantId, new Money("EUR", 42.66), new PaymentGateway.Infrastructure.Card("4524 4587 5698 1200", "05/19", "321"));
        }

        public static PaymentRequest BuildInvalidCardNumberPaymentRequest(Guid requestId, string invalidCardNumber)
        {
            return new PaymentRequest(requestId, MerchantsRepository.Amazon, new Money("EUR", 42.66), new PaymentGateway.Infrastructure.Card(invalidCardNumber, "05/19", "321"));
        }

        public static PaymentRequest BuildInvalidCardCvvPaymentRequest(Guid requestId, string invalidCvv)
        {
            return new PaymentRequest(requestId, MerchantsRepository.Amazon, new Money("EUR", 42.66), new PaymentGateway.Infrastructure.Card("0214 4587 5698 1200", "05/19", invalidCvv));
        }

        public static PaymentRequest BuildInvalidCardExpiryPaymentRequest(Guid requestId, string invalidExpiry)
        {
            return new PaymentRequest(requestId, MerchantsRepository.Amazon, new Money("EUR", 42.66), new PaymentGateway.Infrastructure.Card("0214 4587 5698 1200", invalidExpiry, "325"));
        }
    }
}