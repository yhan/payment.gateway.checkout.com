using System;
using System.Threading.Tasks;

namespace AcquiringBanks.API
{

    /// <summary>
    /// Acquiring bank abstraction which allows `Gateway` to different merchants' banks
    /// an request payments
    /// </summary>
    public interface IAmAcquiringBank
    {
        Task<BankResponse> RespondsTo(PayingAttempt paymentAttempt);
        Task<bool> Connect();
    }

    public class BankResponse : IBankResponse
    {
        public Guid BankPaymentId { get; }
        public Guid GatewayPaymentId { get; }
        public AcquiringBanks.API.BankPaymentStatus PaymentStatus { get; }

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