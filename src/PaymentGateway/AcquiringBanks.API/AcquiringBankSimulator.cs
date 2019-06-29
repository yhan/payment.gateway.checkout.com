using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcquiringBanks.API
{
    public class AcquiringBankSimulator : IAmAcquiringBank
    {
        private readonly IRandomnizeAcquiringBankPaymentStatus _random;
        private readonly IGenerateBankPaymentId _bankPaymentIdGenerator;
        private readonly IProvideRandomBankResponseTime _delayProvider;

        public AcquiringBankSimulator(IRandomnizeAcquiringBankPaymentStatus random, IGenerateBankPaymentId bankPaymentIdGenerator, IProvideRandomBankResponseTime delayProvider)
        {
            _random = random;
            _bankPaymentIdGenerator = bankPaymentIdGenerator;
            _delayProvider = delayProvider;
        }

        public async Task<string> RespondsTo(string paymentAttemptJson)
        {
            var payingAttempt = JsonConvert.DeserializeObject<AcquiringBanks.API.PayingAttempt>(paymentAttemptJson);

            await Task.Delay(_delayProvider.Delays());

            var paymentStatus = _random.GeneratePaymentStatus();

            var bankPaymentId = _bankPaymentIdGenerator.Generate();
            var response = new Response(bankPaymentId, payingAttempt.GatewayPaymentId, paymentStatus);

            return JsonConvert.SerializeObject(response);
        }
    }
}