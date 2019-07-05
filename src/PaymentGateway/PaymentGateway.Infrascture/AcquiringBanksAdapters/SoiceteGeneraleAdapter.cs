using System;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class SoiceteGeneraleAdapter : AdaptToBank
    {
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;
        private readonly SocieteGenerale _societeGenerale;

        public SoiceteGeneraleAdapter(IProvideBankResponseTime delayProvider,
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

        protected override async Task<IBankResponse> CallBank(PayingAttempt payingAttempt, CancellationToken cancellationToken)
        {
            // Simulate bank response delay
            var delay = DelayProvider.Delays();
            await Task.Delay(delay, cancellationToken);
            
            // Call bank's service
            var request = new SocieteGeneralePaymentRequest(payingAttempt.GatewayPaymentId, payingAttempt.Amount, payingAttempt.Currency, payingAttempt.CardCvv, payingAttempt.CardExpiry, payingAttempt.CardNumber);
            SocieteGeneraleResponse response = await _societeGenerale.RespondToPayment(request);
            
            _paymentIdsMapper.RememberMapping(new AcquiringBankPaymentId(response.BankPaymentId), new GatewayPaymentId(response.GatewayPaymentId) );

            return AdaptToBankResponse(response);
        }
    }
    
    public class StupidBankAlwaysSendTheSamePaymentId : AdaptToBank
    {
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;

        public StupidBankAlwaysSendTheSamePaymentId(IProvideBankResponseTime delayProvider, IMapAcquiringBankToPaymentGateway paymentIdsMapper, ILogger<BankAdapterSelector> logger) : base(delayProvider, logger)
        {
            _paymentIdsMapper = paymentIdsMapper;
        }

        public override async Task<bool> Connect()
        {
            return await Task.FromResult(true);
        }

        protected override async Task<IBankResponse> CallBank(PayingAttempt payingAttempt, CancellationToken cancellationToken)
        {
            var neverChangingBankPaymentId = Guid.Parse("d6589823-2bfa-4e1a-9f3f-699cb9a0a0a2");
            var response = new BankResponse(bankPaymentId: neverChangingBankPaymentId, gatewayPaymentId: payingAttempt.GatewayPaymentId, paymentStatus: BankPaymentStatus.Accepted);
            
            _paymentIdsMapper.RememberMapping(new AcquiringBankPaymentId(neverChangingBankPaymentId), new GatewayPaymentId(response.GatewayPaymentId));
            
            return await Task.FromResult(response);
        }
    }
}