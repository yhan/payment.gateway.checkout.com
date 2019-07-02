using System;
using System.Threading.Tasks;
using AcquiringBanks.API;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using Polly;
using PayingAttempt = PaymentGateway.Domain.AcquiringBank.PayingAttempt;

namespace PaymentGateway.Infrastructure
{
    public interface IAdaptToBank
    {
        Task<IBankResponse> RespondToPaymentAttempt(PayingAttempt paymentAttempt);
    }

    public abstract class AdaptToBank : IAdaptToBank
    {
        protected readonly IRandomnizeAcquiringBankPaymentStatus Random;
        protected readonly IGenerateBankPaymentId BankPaymentIdGenerator;
        protected readonly IProvideRandomBankResponseTime DelayProvider;
        private readonly IConnectToAcquiringBanks _connectionBehavior;
        protected readonly ILogger<BankAdapterSelector> Logger;

        protected AdaptToBank(IRandomnizeAcquiringBankPaymentStatus random,
            IGenerateBankPaymentId bankPaymentIdGenerator,
            IProvideRandomBankResponseTime delayProvider,
            IConnectToAcquiringBanks connectionBehavior,
            ILogger<BankAdapterSelector> logger)
        {
            Random = random;
            BankPaymentIdGenerator = bankPaymentIdGenerator;
            DelayProvider = delayProvider;
            _connectionBehavior = connectionBehavior;
            Logger = logger;
        }

        public async Task<IBankResponse> RespondToPaymentAttempt(PayingAttempt paymentAttempt)
        {
            // Connection to bank
            var policy = Policy.Handle<FailedConnectionToBankException>()
                .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(Math.Pow(2, retry)));

            var policyResult = await policy.ExecuteAndCaptureAsync(async () => await Connect());
            if (!policyResult.Result)
            {
                return new BankDoesNotRespond(paymentAttempt.GatewayPaymentId);
            }

           
            // adapt `paymentAttempt` to  accepted format of a specific bank
            // network call the specific bank's payment endpoint
            return await CallBank( paymentAttempt.GatewayPaymentId);
        }

        public async Task<bool> Connect()
        {
            return await _connectionBehavior.Connect();
        }

        protected abstract Task<IBankResponse> CallBank(Guid gatewayPaymentId);
    }

    public class SoiceteGeneraleAdapter : AdaptToBank
    {
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;
        
        public SoiceteGeneraleAdapter(IRandomnizeAcquiringBankPaymentStatus random, 
            IGenerateBankPaymentId bankPaymentIdGenerator, 
            IProvideRandomBankResponseTime delayProvider,
            IConnectToAcquiringBanks connectionBehavior,
            IMapAcquiringBankToPaymentGateway paymentIdsMapper,
            ILogger<BankAdapterSelector> logger ) : base(random, bankPaymentIdGenerator, delayProvider, connectionBehavior, logger)
        {
            _paymentIdsMapper = paymentIdsMapper;
        }

        private IBankResponse AdaptToBankResponse(SocieteGeneraleResponse response)
        {
            return new BankResponse(response.BankPaymentId, response.GatewayPaymentId, response.PaymentStatus);
        }

        protected override async Task<IBankResponse> CallBank(Guid gatewayPaymentId)
        {
            // Simulate bank response delay
            var delay = DelayProvider.Delays();
            await Task.Delay(delay);
            Logger.LogInformation($"Bank delayed {delay}");

            // generate random payment status
            var paymentStatus = Random.GeneratePaymentStatus();

            // generate random bank's payment id
            var bankPaymentId = BankPaymentIdGenerator.Generate();

            var response = new SocieteGeneraleResponse(bankPaymentId, gatewayPaymentId, paymentStatus);

            _paymentIdsMapper.RememberMapping(new AcquiringBankPaymentId(bankPaymentId), new GatewayPaymentId(gatewayPaymentId) );

            return AdaptToBankResponse(response);
        }
    }

    public class BNPAdapter : AdaptToBank
    {
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;

        public BNPAdapter(IRandomnizeAcquiringBankPaymentStatus random,
            IGenerateBankPaymentId bankPaymentIdGenerator, 
            IProvideRandomBankResponseTime delayProvider,
            IConnectToAcquiringBanks connectionBehavior,
            IMapAcquiringBankToPaymentGateway paymentIdsMapper,
            ILogger<BankAdapterSelector> logger)
            : base(random, bankPaymentIdGenerator, delayProvider, connectionBehavior, logger)
        {
            _paymentIdsMapper = paymentIdsMapper;
        }

        private IBankResponse AdaptToBankResponse(BNPResponse response)
        {
            return new BankResponse(response.BankPaymentId, response.GatewayPaymentId, response.PaymentStatus);
        }

        protected override async Task<IBankResponse> CallBank(Guid gatewayPaymentId)
        {
            // Simulate bank response delay
            var delay = DelayProvider.Delays();
            await Task.Delay(delay);
            Logger.LogInformation($"Bank delayed {delay}");

            var paymentStatus = Random.GeneratePaymentStatus();
            var bankPaymentId = BankPaymentIdGenerator.Generate();

            var response = new BNPResponse(bankPaymentId, gatewayPaymentId, paymentStatus);
            _paymentIdsMapper.RememberMapping(new AcquiringBankPaymentId(bankPaymentId), new GatewayPaymentId(gatewayPaymentId) );
            return AdaptToBankResponse(response);
        }
    }
}