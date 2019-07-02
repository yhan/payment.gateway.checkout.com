using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentGateway.Domain.Events;

namespace PaymentGateway.Infrastructure
{
    public interface IEventStore
    {
        Task SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion);

        Task<List<Event>> GetEventsForAggregate(Guid aggregateId);
    }

    public class InMemoryEventStore : IEventStore
    {
        private readonly IPublishEvents _publisher;

        private struct EventDescriptor
        {

            public readonly Event EventData;
            public readonly Guid Id;
            public readonly int Version;

            public EventDescriptor(Guid id, Event eventData, int version)
            {
                EventData = eventData;
                Version = version;
                Id = id;
            }
        }

        public InMemoryEventStore(IPublishEvents publisher)
        {
            _publisher = publisher;
        }

        //https://stackoverflow.com/questions/28896997/nullreferenceexception-when-adding-to-dictionary-in-asynchronous-context
        private readonly ConcurrentDictionary<Guid, ConcurrentBag<EventDescriptor>> _current = new ConcurrentDictionary<Guid, ConcurrentBag<EventDescriptor>>();

        public async Task SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            // Simulate I/O, avoid blocking thread pool thread
            await Task.CompletedTask;

            ConcurrentBag<EventDescriptor> eventDescriptors;

            // try to get event descriptors list for given aggregate id
            // otherwise -> create empty dictionary
            if(!_current.TryGetValue(aggregateId, out eventDescriptors))
            {
                eventDescriptors = new ConcurrentBag<EventDescriptor>();
                _current.TryAdd(aggregateId,eventDescriptors);
                
            }
            // check whether latest event version matches current aggregate version
            // otherwise -> throw exception
            else if(eventDescriptors.Last().Version != expectedVersion && expectedVersion != -1)//[eventDescriptors.Count - 1]
            {
                throw new ConcurrencyException();
            }
            var i = expectedVersion;

            // iterate through current aggregate events increasing version with each processed event
            foreach (var @event in events)
            {
                i++;
                @event.Version = i;

                // push event to the event descriptors list for current aggregate
                eventDescriptors.Add(new EventDescriptor(aggregateId,@event,i));

                // publish current event to the bus for further processing by subscribers
                await _publisher.Publish(@event);
            }
        }

        // collect all processed events for given aggregate and return them as a list
        // used to build up an aggregate from its history (Domain.LoadsFromHistory)
        public async Task<List<Event>> GetEventsForAggregate(Guid aggregateId)
        {
            await Task.CompletedTask;

            if (!_current.TryGetValue(aggregateId, out var eventDescriptors))
            {
                throw new AggregateNotFoundException();
            }

            return eventDescriptors.Reverse().Select(desc => desc.EventData).ToList();
        }
    }

    public class AggregateNotFoundException : Exception
    {
    }

    public class ConcurrencyException : Exception
    {
    }
}
