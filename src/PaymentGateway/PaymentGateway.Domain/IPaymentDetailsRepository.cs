using System;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    public interface IPaymentDetailsRepository
    {
        Task<PaymentDetails> GetPaymentDetails(GatewayPaymentId paymentGatewayId);

        Task Create(GatewayPaymentId gatewayPaymentId, CreditCard creditCard);

        Task Update(GatewayPaymentId gatewayPaymentId, AcquiringBankPaymentId bankPaymentId, PaymentStatus paymentStatus);
    }
}