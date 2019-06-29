using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AcquiringBanks.API
{
    public class AcquiringBankSimulator : IAmAcquiringBank
    {
        private readonly IRandomnizeAcquiringBankPaymentStatus _random;
        private readonly IGenerateBankPaymentId _bankPaymentIdGenerator;

        public AcquiringBankSimulator(IRandomnizeAcquiringBankPaymentStatus random, IGenerateBankPaymentId bankPaymentIdGenerator)
        {
            _random = random;
            _bankPaymentIdGenerator = bankPaymentIdGenerator;
        }

        public async Task<string> RespondsTo(string paymentAttemptJson)
        {
            var payingAttempt = JsonConvert.DeserializeObject<AcquiringBanks.API.PayingAttempt>(paymentAttemptJson);

            await Task.Delay(1);

            var paymentStatus = _random.GeneratePaymentStatus();

            var bankPaymentId = _bankPaymentIdGenerator.Generate();
            var response = new Response(bankPaymentId, payingAttempt.GatewayPaymentId, paymentStatus);

            return JsonConvert.SerializeObject(response);
        }
    }

    public interface IGenerateBankPaymentId
    {
        Guid Generate();
    }

    public class DefaultBankPaymentIdGenerator : IGenerateBankPaymentId
    {
        public Guid Generate()
        {
            return Guid.NewGuid();
        }
    }
}