
namespace PaymentGateway.Infrastructure
{
    using System.Threading.Tasks;
    using Domain;


    public interface IAdaptToBank
    {
        Task<IBankResponse> RespondToPaymentAttempt(PayingAttempt paymentAttempt);
    }
}