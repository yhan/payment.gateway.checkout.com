using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    public interface IProcessPayment
    {
        Task AttemptPaying(PayingAttempt payingAttempt);
    }
}