using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleCQRS;

namespace PaymentGateway.Infrastructure
{
    public class EventSourcedRepository<T> : IEventSourcedRepository<T> where T: AggregateRoot, new() //shortcut you can do as you see fit with new()
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

            //aggregate.UpdateVersion(uncommittedChanges.Last().Version);
        }

        public async Task<T> GetById(Guid id)
        {
            var obj = new T();//lots of ways to do this
            var e = await  _storage.GetEventsForAggregate(id);
            obj.LoadsFromHistory(e);
            return obj;
        }
    }
}