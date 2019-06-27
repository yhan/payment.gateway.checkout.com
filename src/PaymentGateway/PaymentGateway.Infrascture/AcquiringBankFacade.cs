using System;
using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class AcquiringBankFacade : ITalkToAcquiringBank
    {
        public async Task<BankResponse> Pay(Payment payment)
        {
            await Task.Delay(1);
            return new BankResponse(payment.GatewayPaymentId, Guid.NewGuid(), PaymentStatus.Success);
        }
    }
}