using System;
using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    // In distributed system, the cache will be a external shared one.
    // We will have I/O, here simulate an I/O
    public class InMemoryPaymentIdsMapping : IProvidePaymentIdsMapping
    {
        private readonly ConcurrentHashSet<Guid> _paymentRequests = new ConcurrentHashSet<Guid>();

        public async Task<bool> AlreadyHandled(Guid paymentRequestId)
        {

            await Task.CompletedTask;

            return _paymentRequests.Contains(paymentRequestId);
        }

        public async Task Remember(Guid paymentRequestId)
        {
            await Task.CompletedTask;

            _paymentRequests.Add(paymentRequestId);
        }
    }
}