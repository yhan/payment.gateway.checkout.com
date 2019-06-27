using System;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{

    public interface IProvidePaymentIdsMapping
    {
        Task<bool> AlreadyHandled(Guid paymentRequestId);
        Task Remember(Guid paymentRequestId);
    }
}