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
        Task<BankResponse> Pay(PayingAttempt paymentAttempt);
    }

    public class BankResponse
    {
        public Guid BankPaymentId { get; }
        public Guid GatewayPaymentId { get; }
        public BankPaymentStatus PaymentStatus { get; }

        public BankResponse(Guid bankPaymentId, Guid gatewayPaymentId, BankPaymentStatus paymentStatus)
        {
            BankPaymentId = bankPaymentId;
            GatewayPaymentId = gatewayPaymentId;
            PaymentStatus = paymentStatus;
        }
    }
}