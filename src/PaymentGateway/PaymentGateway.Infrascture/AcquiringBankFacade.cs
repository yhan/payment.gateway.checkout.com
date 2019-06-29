using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcquiringBanks.API;
using Newtonsoft.Json;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
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
            //Send `PayingAttempt` to AcquiringBank, wait, get reply

            string bankResponseJson = await _bank.RespondsTo(JsonConvert.SerializeObject(paymentAttempt));
            
            var bankResponse = JsonConvert.DeserializeObject<PaymentGateway.Domain.BankResponse>(bankResponseJson);

            _paymentIdsMapper.RememberMapping(new PaymentIds(bankResponse.BankPaymentId, bankResponse.GatewayPaymentId));

            return bankResponse;
        }
    }

   
}