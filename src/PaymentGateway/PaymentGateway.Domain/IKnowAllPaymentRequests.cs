using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    /// <summary>
    /// Cache payment request known by the system (distinguished by their ids)
    /// </summary>
    public interface IKnowAllPaymentRequests
    {
        Task<bool> AlreadyHandled(PaymentRequestId paymentRequestId);

        Task Remember(PaymentRequestId paymentRequestId);
    }
}