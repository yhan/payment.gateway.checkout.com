using System;

namespace PaymentGateway.Domain.Events
{
    public abstract class Event : IMessage
    {
        public int Version;
    }

    /// <summary>
    /// Event occurred on an `Aggregate` or `Entity`
    /// </summary>
    [Serializable]
    public abstract class AggregateEvent : Event
    {
        protected AggregateEvent(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }

        /// <summary>
        /// Gets the aggregate id.
        /// <remarks>Added to ease our event generator</remarks>
        /// </summary>
        public Guid AggregateId { get; set; }
    }
}