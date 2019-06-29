using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentDetailsRepository : IPaymentDetailsRepository
    {
        //GatewayPaymentId => PaymentDetails
        private readonly ConcurrentDictionary<Guid, PaymentDetails> _storage = new ConcurrentDictionary<Guid, PaymentDetails>();

        public async Task Create(Guid gatewayPaymentId, CreditCard creditCard)
        {
            //Simulate IO
            await Task.Delay(1);

            if (!_storage.TryAdd(gatewayPaymentId,
                new PaymentDetails(gatewayPaymentId, creditCard.HolderName, Mask(creditCard.Number), creditCard.Expiry,
                    creditCard.Cvv)))
            {
                throw new ConstraintException($"Payment with GatewayId '{gatewayPaymentId}' already created");
            }
        }

        private static string Mask(string creditCardNumber)
        {
            var mask = creditCardNumber.Select((c, i) =>
            {
                if (i <= 3) return c;
                if (c == ' ') return c;
                return 'X';
            }).ToArray();

            return new string(mask);
        }

        public async Task Update(Guid gatewayPaymentId, Guid bankPaymentId, PaymentStatus paymentStatus)
        {
            //Simulate IO
            await Task.Delay(1);

            var paymentDetails = _storage[gatewayPaymentId];
            paymentDetails.Update(bankPaymentId, paymentStatus);

        }

        public async Task<PaymentDetails> GetPaymentDetails(Guid paymentGatewayId)
        {
            //Simulate IO
            await Task.Delay(1);

            return _storage[paymentGatewayId];
        }
    }
}