using System.Threading.Tasks;
using AcquiringBanks.API;

namespace PaymentGateway.Infrastructure
{
    public class AlwaysFailBankConnectionBehavior : IConnectToAcquiringBanks
    {
        public Task<bool> Connect()
        {
            throw new FailedConnectionToBankException();
        }
    }
}