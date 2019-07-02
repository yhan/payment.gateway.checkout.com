using System.Threading.Tasks;
using AcquiringBanks.Stub;

namespace PaymentGateway.Tests
{
    public class AlwaysFailBankConnectionBehavior : IConnectToAcquiringBanks
    {
        public Task<bool> Connect()
        {
            throw new FailedConnectionToBankException();
        }
    }
}