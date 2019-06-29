using System;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    public interface IPaymentDetailsRepository
    {
        Task<PaymentDetails> GetPaymentDetails(Guid paymentGatewayId);

        Task Create(Guid gatewayPaymentId, CreditCard creditCard);

        Task Update(Guid gatewayPaymentId, Guid bankPaymentId, PaymentStatus paymentStatus);
    }
}