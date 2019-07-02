using System;
using System.Threading.Tasks;

namespace AcquiringBanks.API
{
    public class RandomConnectionBehavior : IConnectToAcquiringBanks
    {
        private static readonly Random _random = new Random(42);
        private bool _alreadyFailedOnce = false;


        public RandomConnectionBehavior()
        {
        }

        public async Task<bool> Connect()
        {
            var next = _random.Next(0, 101);
            Console.WriteLine($"************   random = {next}   *******************");
            if (next % 5 == 0 || _alreadyFailedOnce)
            {

                _alreadyFailedOnce = true;
                throw new FailedConnectionToBankException();
            }


            return await Task.FromResult(true);
        }
    }


    public interface IConnectToAcquiringBanks
    {
        Task<bool> Connect();
    }

    public class AlwaysSuccessBankConnectionBehavior : IConnectToAcquiringBanks
    {
        public async Task<bool> Connect()
        {
            return await Task.FromResult(true);
        }
    }
}