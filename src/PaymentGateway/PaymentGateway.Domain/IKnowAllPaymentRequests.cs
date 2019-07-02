using System.Threading.Tasks;

namespace PaymentGateway.Domain
{

    public interface IKnowAllPaymentRequests
    {
        Task<bool> AlreadyHandled(PaymentRequestId paymentRequestId);

        Task Remember(PaymentRequestId paymentRequestId);
    }
}