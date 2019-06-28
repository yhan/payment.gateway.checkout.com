using System;
using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{


    public class AcquiringBankFacade : ITalkToAcquiringBank
    {
        private readonly Task _delay = Task.Delay(1);

        public async Task<BankResponse> Pay(Payment payment)
        {
            await _delay;
            return new BankResponse(payment.GatewayPaymentId, Guid.NewGuid(), PaymentStatus.Success);
        }

        internal async Task WaitForBankResponse()
        {
            await _delay;
        }
    }
}