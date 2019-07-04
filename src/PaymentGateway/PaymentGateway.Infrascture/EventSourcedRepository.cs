using System;
using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class EventSourcedRepository<T> : IEventSourcedRepository<T>
        where T : AggregateRoot, new()
    {
        private readonly IEventStore _storage;

        public EventSourcedRepository(IEventStore storage)
        {
            _storage = storage;
        }

        public async Task Save(AggregateRoot aggregate, int expectedVersion)
        {
            var uncommittedChanges = aggregate.GetUncommittedChanges();
            await _storage.SaveEvents(aggregate.Id, uncommittedChanges, expectedVersion);

            aggregate.MarkChangesAsCommitted();
        }

        public async Task<T> GetById(Guid id)
        {
            var obj = new T(); //lots of ways to do this
            var events = await _storage.GetEventsForAggregate(id);
            obj.LoadsFromHistory(events);
            return obj;
        }
    }
}