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
    }
}