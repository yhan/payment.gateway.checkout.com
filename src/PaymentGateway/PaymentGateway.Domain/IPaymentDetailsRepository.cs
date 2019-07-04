using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    /// <summary>
    /// Repository of real models
    /// </summary>
    public interface IPaymentDetailsRepository
    {
        Task<PaymentDetails> GetPaymentDetails(GatewayPaymentId paymentGatewayId);

        Task Create(GatewayPaymentId gatewayPaymentId, Card card);

        Task Update(GatewayPaymentId gatewayPaymentId, AcquiringBankPaymentId bankPaymentId, PaymentStatus paymentStatus);

        Task Update(GatewayPaymentId gatewayPaymentId, PaymentStatus paymentStatus);
    }
}