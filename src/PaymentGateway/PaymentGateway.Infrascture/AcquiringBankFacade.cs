using System;
using System.Threading.Tasks;
using PaymentGateway.Domain;
using PaymentGateway.Domain.AcquiringBank;

namespace PaymentGateway.Infrastructure
{
    public class AcquiringBankFacade : ITalkToAcquiringBank
    {
        private readonly IRandomnizeAcquiringBankPaymentStatus _random;
        private readonly Task _delay = Task.Delay(1);

        public AcquiringBankFacade(IRandomnizeAcquiringBankPaymentStatus random)
        {
            _random = random;
        }

        public async Task<BankResponse> Pay(PayingAttempt payment)
        {
            //Send `PayingAttempt` to AcquiringBank, wait, get reply
            await _delay;

            BankPaymentStatus bankPaymentStatus = _random.GeneratePaymentStatus();

            return new BankResponse(payment.GatewayPaymentId, Guid.NewGuid(), bankPaymentStatus);
        }

        internal async Task WaitForBankResponse()
        {
            await _delay;
        }
    }
}