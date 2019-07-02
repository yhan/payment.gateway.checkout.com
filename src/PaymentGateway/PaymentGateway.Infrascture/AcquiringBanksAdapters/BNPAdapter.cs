using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
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