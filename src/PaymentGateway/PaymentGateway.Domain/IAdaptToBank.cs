
using System.Threading;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    /// <summary>
    /// Send <see cref="PayingAttempt"/> to proper acquiring bank and get response.
    /// </summary>
    public interface IAdaptToBank
    {
        Task<IBankResponse> RespondToPaymentAttempt(PayingAttempt paymentAttempt, CancellationToken cancellationToken);
    }
}