using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcquiringBanks.API
{
    public class AcquiringBankSimulator : IAmAcquiringBank
    {
        private readonly IRandomnizeAcquiringBankPaymentStatus _random;

        public AcquiringBankSimulator(IRandomnizeAcquiringBankPaymentStatus random)
        {
            _random = random;
        }

        public async Task<string> RespondsTo(string paymentAttemptJson)
        {
            var payingAttempt = JsonConvert.DeserializeObject<AcquiringBanks.API.PayingAttempt>(paymentAttemptJson);

            await Task.Delay(20);

            var paymentStatus = _random.GeneratePaymentStatus();

            var bankPaymentId = Guid.NewGuid();
            var response = new Response(bankPaymentId, payingAttempt.GatewayPaymentId, paymentStatus);

            return JsonConvert.SerializeObject(response);
        }
    }
}