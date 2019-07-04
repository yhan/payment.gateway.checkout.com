using System.Threading.Tasks;
using AcquiringBanks.Stub;

namespace PaymentGateway.API
{
    public class AlwaysFailBankConnectionBehavior : IConnectToAcquiringBanks
    {
        public Task<bool> Connect()
        {
            throw new FailedConnectionToBankException();
        }
    }
}