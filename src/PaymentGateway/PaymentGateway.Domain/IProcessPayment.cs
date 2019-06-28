using System;
using System.Threading.Tasks;
using PaymentGateway.Domain.AcquiringBank;

namespace PaymentGateway.Domain
{
    public interface IProcessPayment
    {
        Task AttemptPaying(PayingAttempt payingAttempt);
    }

    public interface ITalkToAcquiringBank
    {
        Task<BankResponse> Pay(PayingAttempt payment);
    }

    public class BankResponse
    {
        public Guid GatewayPaymentId { get; }
        public Guid BankPaymentId { get; }
        public BankPaymentStatus PaymentStatus { get; }

        public BankResponse(Guid gatewayPaymentId, Guid bankPaymentId, BankPaymentStatus paymentStatus)
        {
            GatewayPaymentId = gatewayPaymentId;
            BankPaymentId = bankPaymentId;
            PaymentStatus = paymentStatus;
        }
    }
}