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

    /// <inheritdoc />
    /// <summary>
    /// Simulate bank connection success or failure.
    /// For having better demo effect (i.e. To see we can have bank connection failure easily (without too many tries)),
    /// this behaviour will do:
    /// when connection fails the first time, fail it definitively.
    /// </summary>
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