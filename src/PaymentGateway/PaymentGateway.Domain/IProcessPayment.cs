using System.Threading.Tasks;
using PaymentGateway.Domain.AcquiringBank;

namespace PaymentGateway.Domain
{
    public interface IProcessPayment
    {
        Task AttemptPaying(PayingAttempt payingAttempt);
    }
}