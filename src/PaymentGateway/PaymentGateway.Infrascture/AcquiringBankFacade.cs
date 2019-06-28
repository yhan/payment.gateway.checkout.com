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
        private Task<string> _delay;

        public AcquiringBankFacade(IAmAcquiringBank bank)
        {
            _bank = bank;
        }

        public async Task<PaymentGateway.Domain.BankResponse> Pay(PaymentGateway.Domain.AcquiringBank.PayingAttempt paymentAttempt)
        {
            //Send `PayingAttempt` to AcquiringBank, wait, get reply

            _delay = _bank.RespondsTo(JsonConvert.SerializeObject(paymentAttempt));
            string bankResponseJson = await _delay;
            
            var bankResponse = JsonConvert.DeserializeObject<PaymentGateway.Domain.BankResponse>(bankResponseJson);
            
            return bankResponse;
        }

        internal async Task WaitForBankResponse()
        {
            await _delay;
        }
    }
}