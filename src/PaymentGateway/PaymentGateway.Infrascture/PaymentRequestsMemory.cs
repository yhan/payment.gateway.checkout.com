using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    /// <summary>
    /// Represent the memory aware of all <see cref="PaymentRequest"/> ids
    /// </summary>
    public class PaymentRequestsMemory : IKnowAllPaymentRequests
    {
        private readonly ConcurrentHashSet<PaymentRequestId> _paymentRequests = new ConcurrentHashSet<PaymentRequestId>();

        public async Task<bool> AlreadyHandled(PaymentRequestId paymentRequestId)
        {
            // Simulate I/O
            await Task.CompletedTask;

            return _paymentRequests.Contains(paymentRequestId);
        }

        public async Task Remember(PaymentRequestId paymentRequestId)
        {
            // Simulate I/O
            await Task.CompletedTask;

            _paymentRequests.Add(paymentRequestId);
        }
    }
}