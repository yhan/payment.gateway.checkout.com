using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using PaymentGateway.Domain.Events;

namespace PaymentGateway.Infrastructure
{
    public class PaymentDetailsRepository : IPaymentDetailsRepository
    {
        private readonly ILogger<PaymentDetailsRepository> _logger;
        private readonly ConcurrentDictionary<GatewayPaymentId, PaymentDetails> _storage = new ConcurrentDictionary<GatewayPaymentId, PaymentDetails>();

        public PaymentDetailsRepository(ILogger<PaymentDetailsRepository> logger)
        {
            _logger = logger;
        }
        
        public async Task Create(GatewayPaymentId gatewayPaymentId, PaymentGateway.Domain.Card card)
        {
            //Simulate IO
            await Task.Delay(1);

            if (!_storage.TryAdd(gatewayPaymentId, new PaymentDetails(new Domain.Card(Mask(card.Number), card.Expiry, card.Cvv) )))
            {
                // Can happen only if the read projected receive twice the same `PaymentRequested`
                // For a real distributed message bus, it can never deliver once and only once for a message
                // We should ensure the operation on duplicated message is idempotent, just log in this case
                
                _logger.LogInformation($"{nameof(PaymentRequested)} of gateway id {gatewayPaymentId} has already been handled. We received duplicated message from message bus");
            }
        }

        private static string Mask(string cardNumber)
        {
            var mask = cardNumber.Select((c, i) =>
            {
                if (i <= 3) return c;
                if (c == ' ') return c;
                return 'X';
            }).ToArray();

            return new string(mask);
        }

        public async Task Update(GatewayPaymentId gatewayPaymentId, AcquiringBankPaymentId bankPaymentId, PaymentStatus paymentStatus)
        {
            //Simulate IO
            await Task.Delay(1);

            var paymentDetails = _storage[gatewayPaymentId];
            paymentDetails.Update(bankPaymentId, paymentStatus);

        }

        public async Task Update(GatewayPaymentId gatewayPaymentId, PaymentStatus paymentStatus)
        {
            //Simulate IO
            await Task.Delay(1);
            var paymentDetails = _storage[gatewayPaymentId];
            paymentDetails.Update(paymentStatus);
        }

        public async Task<PaymentDetails> GetPaymentDetails(GatewayPaymentId paymentGatewayId)
        {
            //Simulate IO
            await Task.Delay(1);

            return _storage[paymentGatewayId];
        }
    }
}