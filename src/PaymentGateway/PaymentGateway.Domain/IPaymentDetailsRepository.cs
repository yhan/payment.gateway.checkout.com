using System;

namespace PaymentGateway.Domain
{
    public interface IPaymentDetailsRepository
    {
        PaymentDetails GetPaymentDetails(Guid paymentGatewayId);
    }
}