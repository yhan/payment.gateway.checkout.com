using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcquiringBanks.API;
using Newtonsoft.Json;
using PaymentGateway.Domain;
using Polly;
using Polly.Retry;
using PayingAttempt = PaymentGateway.Domain.AcquiringBank.PayingAttempt;

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

        public async Task<PaymentGateway.Domain.IBankResponse> Pay(PaymentGateway.Domain.AcquiringBank.PayingAttempt paymentAttempt)
        {
            // Connection to bank
            var policy = Policy.Handle<FailedConnectionToBankException>()
                               .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(Math.Pow(2, retry)));

            var policyResult = await policy.ExecuteAndCaptureAsync(async () => await _bank.Connect());
            if (!policyResult.Result)
            {
                return new BankDoesNotRespond(paymentAttempt.GatewayPaymentId);
            }


            //Adapt PaymentGateway to AcquiringBank
            string bankResponseJson = await _bank.RespondsTo(JsonConvert.SerializeObject(paymentAttempt));
            
            //Adapt AcquiringBank back to PaymentGateway
            var bankResponse = JsonConvert.DeserializeObject<PaymentGateway.Domain.BankResponse>(bankResponseJson);

            _paymentIdsMapper.RememberMapping(new AcquiringBankPaymentId(bankResponse.BankPaymentId), new GatewayPaymentId(bankResponse.GatewayPaymentId));

            return bankResponse;
        }
    }

    public class BankDoesNotRespond : IBankResponse
    {
        public Guid GatewayPaymentId { get; }

        public BankDoesNotRespond(Guid gatewayPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
        }

        public bool BankContactable()
        {
            return false;
        }
    }

   

    public class FailedConnectionToBankException: Exception
    {
    }

    public interface ISimulateBrokenConnectionToBank
    {
    }
}