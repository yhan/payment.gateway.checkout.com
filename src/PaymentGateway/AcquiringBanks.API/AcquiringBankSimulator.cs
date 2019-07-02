using System;
using System.Threading.Tasks;

namespace AcquiringBanks.API
{
    //public class AcquiringBankSimulator : IAmAcquiringBank
    //{
    //    private readonly IRandomnizeAcquiringBankPaymentStatus _random;
    //    private readonly IGenerateBankPaymentId _bankPaymentIdGenerator;
    //    private readonly IProvideRandomBankResponseTime _delayProvider;
    //    private readonly IConnectToAcquiringBanks _connectionBehavior;
    //    private readonly ILogger<AcquiringBankSimulator> _logger;

    //    public AcquiringBankSimulator(IRandomnizeAcquiringBankPaymentStatus random, 
    //                                  IGenerateBankPaymentId bankPaymentIdGenerator, 
    //                                  IProvideRandomBankResponseTime delayProvider,
    //                                  IConnectToAcquiringBanks connectionBehavior,
    //        ILogger<AcquiringBankSimulator> logger
           

    //        )
    //    {
    //        _random = random;
    //        _bankPaymentIdGenerator = bankPaymentIdGenerator;
    //        _delayProvider = delayProvider;
    //        _connectionBehavior = connectionBehavior;
    //        _logger = logger;
            
    //    }

    //    public async Task<string> RespondsTo(PayingAttempt paymentAttempt)
    //    {
        

    //        //var payingAttempt = JsonConvert.DeserializeObject<AcquiringBanks.API.PayingAttempt>(paymentAttempt);

    //        var delay = _delayProvider.Delays();

    //        _logger.LogInformation($"Bank delayed {delay}");
    //        await Task.Delay(delay);

    //        var paymentStatus = _random.GeneratePaymentStatus();

    //        var bankPaymentId = _bankPaymentIdGenerator.Generate();
    //        var response = new BNPResponse(bankPaymentId, paymentAttempt.GatewayPaymentId, paymentStatus);

    //        return JsonConvert.SerializeObject(response);
    //    }

    //    public async Task<bool> Connect()
    //    {
    //        return await _connectionBehavior.Connect();
    //    }
    //}

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