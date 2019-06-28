using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentDetailsRepository : IPaymentDetailsRepository
    {
        public PaymentDetails GetPaymentDetails(Guid paymentGatewayId)
        {
            throw new NotImplementedException();
        }
    }
}