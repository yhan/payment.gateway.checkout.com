using System;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    public interface IProcessPayment
    {
        Task AttemptPaying(Payment payment);
    }

    public interface ITalkToAcquiringBank
    {
        Task<BankResponse> Pay(Payment payment);
    }

    public class BankResponse
    {
        public Guid GatewayPaymentId { get; }
        public Guid BankPaymentId { get; }
        public PaymentStatus PaymentStatus { get; }

        public BankResponse(Guid gatewayPaymentId, Guid bankPaymentId, PaymentStatus paymentStatus)
        {
            GatewayPaymentId = gatewayPaymentId;
            BankPaymentId = bankPaymentId;
            PaymentStatus = paymentStatus;
        }
    }
}