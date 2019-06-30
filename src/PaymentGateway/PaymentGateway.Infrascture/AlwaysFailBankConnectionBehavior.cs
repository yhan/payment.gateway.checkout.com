using System.Threading.Tasks;
using AcquiringBanks.API;

namespace PaymentGateway.Infrastructure
{
    public class AlwaysFailBankConnectionBehavior : IBankConnectionBehavior
    {
        public Task<bool> Connect()
        {
            throw new FailedConnectionToBankException();
        }
    }
}