using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcquiringBanks.API;
using Newtonsoft.Json;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    /// <summary>
    /// Adapter for AcquiringBank API and PaymentGateway API
    /// </summary>
    public class AcquiringBankFacade : ITalkToAcquiringBank
    {
        private readonly IAmAcquiringBank _bank;
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;

        public AcquiringBankFacade(IAmAcquiringBank bank, IMapAcquiringBankToPaymentGateway paymentIdsMapper)
        {
            _bank = bank;
            _paymentIdsMapper = paymentIdsMapper;
        }

        public async Task<PaymentGateway.Domain.BankResponse> Pay(PaymentGateway.Domain.AcquiringBank.PayingAttempt paymentAttempt)
        {
            //Adapt PaymentGateway to AcquiringBank
            string bankResponseJson = await _bank.RespondsTo(JsonConvert.SerializeObject(paymentAttempt));
            
            //Adapt AcquiringBank back to PaymentGateway
            var bankResponse = JsonConvert.DeserializeObject<PaymentGateway.Domain.BankResponse>(bankResponseJson);

            _paymentIdsMapper.RememberMapping(new AcquiringBankPaymentId(bankResponse.BankPaymentId), new GatewayPaymentId(bankResponse.GatewayPaymentId));

            return bankResponse;
        }
    }

   
}