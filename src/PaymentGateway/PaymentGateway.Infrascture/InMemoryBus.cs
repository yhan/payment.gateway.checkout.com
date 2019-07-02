using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentGateway.Domain;
using PaymentGateway.Domain.Commands;
using PaymentGateway.Domain.Events;

namespace PaymentGateway.Infrastructure
{
    /// <summary>
    /// Represent a in memory message bus.
    /// Can publish both <see cref="Event"/> and <see cref="Command"/>
    /// </summary>
    public class InMemoryBus : ISendCommands, IPublishEvents
    {
        private readonly Dictionary<Type, List<Func<IMessage, Task>>> _routes = new Dictionary<Type, List<Func<IMessage, Task>>>();

        public void RegisterHandler<T>(Func<T, Task> handler) where T : IMessage
        {
            if(!_routes.TryGetValue(typeof(T), out var handlers))
            {
                handlers = new List<Func<IMessage, Task>>();
                _routes.Add(typeof(T), handlers);
            }

            handlers.Add(x => handler((T)x));
        }

        public void UnRegisterHandlers()
        {
            _routes.Clear();
        }

        public void Send<T>(T command) where T : Command
        {
            if (_routes.TryGetValue(typeof(T), out var handlers))
            {
                if (handlers.Count != 1)
                {
                    throw new InvalidOperationException("cannot send to more than one handler");
                }

                handlers[0](command);
            }
            else
            {
                throw new InvalidOperationException("no handler registered");
            }
        }

        public async Task Publish<T>(T @event) where T : Event
        {
            if (!_routes.TryGetValue(@event.GetType(), out var asyncHandlers))
            {
                return;
            }

            foreach(var handler in asyncHandlers)
            {
                var handler1 = handler;

                await handler1(@event);
            }
        }
    }
    
    public interface IHandles<T>
    {
        void Handle(T message);
    }

    public interface ISendCommands
    {
        void Send<T>(T command) where T : Command;

    }
    public interface IPublishEvents
    {
        Task Publish<T>(T @event) where T : Event;

        void RegisterHandler<T>(Func<T, Task> handler) where T : IMessage;

        void UnRegisterHandlers();
    }
}
