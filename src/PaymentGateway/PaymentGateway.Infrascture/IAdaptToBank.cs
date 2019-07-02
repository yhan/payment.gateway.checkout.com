using System;
using System.Threading.Tasks;
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
        protected readonly IProvideRandomBankResponseTime DelayProvider;
        protected readonly ILogger<BankAdapterSelector> Logger;

        protected AdaptToBank(IProvideRandomBankResponseTime delayProvider, ILogger<BankAdapterSelector> logger)
        {
            DelayProvider = delayProvider;
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
            
            return await CallBank( paymentAttempt);
        }

        public abstract Task<bool> Connect();
        //{
        //    return await _connectionBehavior.Connect();
        //}

        protected abstract Task<IBankResponse> CallBank(PayingAttempt payingAttempt);
    }

    public class SoiceteGeneraleAdapter : AdaptToBank
    {
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;
        private readonly SocieteGenerale _societeGenerale;

        public SoiceteGeneraleAdapter(IProvideRandomBankResponseTime delayProvider,
            IConnectToAcquiringBanks connectionBehavior,
            IMapAcquiringBankToPaymentGateway paymentIdsMapper,
            SocieteGenerale societeGenerale,
            ILogger<BankAdapterSelector> logger) : base(delayProvider, logger)
        {
            _paymentIdsMapper = paymentIdsMapper;
            _societeGenerale = societeGenerale;
        }

        private IBankResponse AdaptToBankResponse(SocieteGeneraleResponse response)
        {
            return new BankResponse(response.BankPaymentId, response.GatewayPaymentId, response.PaymentStatus);
        }

        public override async Task<bool> Connect()
        {
            return  await _societeGenerale.Connect();
        }

        protected override async Task<IBankResponse> CallBank(PayingAttempt payingAttempt)
        {
            // Simulate bank response delay
            var delay = DelayProvider.Delays();
            await Task.Delay(delay);
            Logger.LogInformation($"Bank delayed {delay}");
            
            // Call bank's service
            var request = new SocieteGeneralePaymentRequest(payingAttempt.GatewayPaymentId, payingAttempt.Amount, payingAttempt.Currency, payingAttempt.CardCvv, payingAttempt.CardExpiry, payingAttempt.CardNumber);
            SocieteGeneraleResponse response = await _societeGenerale.RespondToPayment(request);
            
            _paymentIdsMapper.RememberMapping(new AcquiringBankPaymentId(response.BankPaymentId), new GatewayPaymentId(response.GatewayPaymentId) );

            return AdaptToBankResponse(response);
        }
    }

    public class BNPAdapter : AdaptToBank
    {
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;
        private readonly BNP _bnp;

        public BNPAdapter(IProvideRandomBankResponseTime delayProvider,
            IConnectToAcquiringBanks connectionBehavior,
            IMapAcquiringBankToPaymentGateway paymentIdsMapper,
            BNP bnp,
            ILogger<BankAdapterSelector> logger)
            : base(delayProvider, logger)
        {
            _paymentIdsMapper = paymentIdsMapper;
            _bnp = bnp;
        }

        private IBankResponse AdaptToBankResponse(BNPResponse response)
        {
            return new BankResponse(response.BankPaymentId, response.GatewayPaymentId, response.PaymentStatus);
        }

        public override async Task<bool> Connect()
        {
            return  await _bnp.Connect();
        }

        protected override async Task<IBankResponse> CallBank(PayingAttempt payingAttempt)
        {
            // Simulate bank response delay
            var delay = DelayProvider.Delays();
            await Task.Delay(delay);
            Logger.LogInformation($"Bank delayed {delay}");

            // Call bank's service
            var request = new BNPPaymentRequest(payingAttempt.GatewayPaymentId, payingAttempt.Amount, payingAttempt.Currency, payingAttempt.CardCvv, payingAttempt.CardExpiry, payingAttempt.CardNumber);
            BNPResponse response = await _bnp.RespondToPayment(request);
            
            _paymentIdsMapper.RememberMapping(new AcquiringBankPaymentId(response.BankPaymentId), new GatewayPaymentId(response.GatewayPaymentId) );
            return AdaptToBankResponse(response);
        }
    }

}