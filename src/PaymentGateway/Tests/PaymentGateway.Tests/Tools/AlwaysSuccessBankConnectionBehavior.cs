using System.Threading.Tasks;
using AcquiringBanks.Stub;

namespace PaymentGateway.Tests
{
    public class AlwaysSuccessBankConnectionBehavior : IConnectToAcquiringBanks
    {
        public async Task<bool> Connect()
        {
            return await Task.FromResult(true);
        }
    }
}