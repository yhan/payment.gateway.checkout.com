using System;
using System.Threading.Tasks;

namespace AcquiringBanks.Stub
{
    /// <summary>
    /// Connect to acquiring bank
    /// </summary>
    public interface IConnectToAcquiringBanks
    {
        Task<bool> Connect();
    }

    /// <summary>
    /// Simulate bank connection success or failure
    /// </summary>
    public class RandomConnectionBehavior : IConnectToAcquiringBanks
    {
        private static readonly Random Random = new Random(42);

        /// <summary>
        /// For runtime manual test purpose,
        /// testing scenario is when connection fails the first time, fail it definitively.
        /// To see we can have bank connection failure easily (without too many tries)
        /// </summary>
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