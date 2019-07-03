using System;

namespace PaymentGateway.Domain
{
    public class BankOnboardMissingException : Exception
    {
        public BankOnboardMissingException(Guid merchantId): base($"Merchant {merchantId} has not been onboarded")
        {
            
        }
    }
}