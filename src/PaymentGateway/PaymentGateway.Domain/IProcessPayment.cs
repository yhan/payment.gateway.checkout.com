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
        Task<IBankResponse> Pay(PayingAttempt paymentAttempt);
    }

    public class BankResponse : IBankResponse
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

        public bool BankContactable()
        {
            return true;
        }
    }

    public interface IBankResponse
    {
        bool BankContactable();
    }
}