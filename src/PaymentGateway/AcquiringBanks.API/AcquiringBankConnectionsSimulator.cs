using System;
using System.Threading.Tasks;

namespace AcquiringBanks.Stub
{
    public interface IConnectToAcquiringBanks
    {
        Task<bool> Connect();
    }

    public class RandomConnectionBehavior : IConnectToAcquiringBanks
    {
        private static readonly Random Random = new Random(42);
        private bool _alreadyFailedOnce;

        public async Task<bool> Connect()
        {
            var next = Random.Next(0, 101);
            if (next % 5 == 0 || _alreadyFailedOnce)
            {
                _alreadyFailedOnce = true;
                throw new FailedConnectionToBankException();
            }
            
            return await Task.FromResult(true);
        }
    }
}