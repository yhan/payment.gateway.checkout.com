using System;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{

    public interface IProvidePaymentIdsMapping
    {
        Task<bool> AlreadyHandled(PaymentRequestId paymentRequestId);

        Task Remember(PaymentRequestId paymentRequestId);
    }
}