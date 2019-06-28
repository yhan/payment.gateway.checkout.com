using System;
using System.Collections.Generic;
using System.Threading;
using SimpleCQRS;

namespace PaymentGateway.Infrastructure
{
    public class InMemoryBus : ICommandSender, IEventPublisher
    {

        public InMemoryBus(bool synchronousPublication = true)
        {
            if (synchronousPublication)
            {
                _publicationStrategy = new SynchronousPublicationStrategy();
            }
            else
            {
                _publicationStrategy = new AsynchronousThreadPoolPublicationStrategy();
            }
        }

        private readonly Dictionary<Type, List<Action<Message>>> _routes = new Dictionary<Type, List<Action<Message>>>();
        private readonly IPublishToHandlers _publicationStrategy;

        public void RegisterHandler<T>(Action<T> handler) where T : Message
        {
            List<Action<Message>> handlers;

            if(!_routes.TryGetValue(typeof(T), out handlers))
            {
                handlers = new List<Action<Message>>();
                _routes.Add(typeof(T), handlers);
            }

            handlers.Add((x => handler((T)x)));
        }

        public void Send<T>(T command) where T : Command
        {
            List<Action<Message>> handlers;

            if (_routes.TryGetValue(typeof(T), out handlers))
            {
                if (handlers.Count != 1)
                {
                    throw new InvalidOperationException("cannot send to more than one handler");
                }

                _publicationStrategy.PublishTo(handlers[0], command);
            }
            else
            {
                throw new InvalidOperationException("no handler registered");
            }
        }

        public void Publish<T>(T @event) where T : Event
        {
            List<Action<Message>> handlers;

            if (!_routes.TryGetValue(@event.GetType(), out handlers)) return;

            foreach(var handler in handlers)
            {
                //dispatch on thread pool for added awesomeness
                var handler1 = handler;

                _publicationStrategy.PublishTo(handler1, @event);
            }
        }
    }

    internal interface IPublishToHandlers 
    {
        void PublishTo<T>(Action<Message> handler, T @event)
            where T : Message;
    }

    public class AsynchronousThreadPoolPublicationStrategy : IPublishToHandlers
    {
        public void PublishTo<T>(Action<Message> handler, T @event)
            where T : Message
        {
            // dispatch on thread pool for added awesomeness
            ThreadPool.QueueUserWorkItem(x => handler(@event));
        }
    }

    public class SynchronousPublicationStrategy: IPublishToHandlers
    {
        public void PublishTo<T>(Action<Message> handler, T @event)
            where T : Message
        {
            handler(@event); // synchronous publication to simplify the test of this first step
        }
    }

    public interface Handles<T>
    {
        void Handle(T message);
    }

    public interface ICommandSender
    {
        void Send<T>(T command) where T : Command;

    }
    public interface IEventPublisher
    {
        void Publish<T>(T @event) where T : Event;
    }
}
