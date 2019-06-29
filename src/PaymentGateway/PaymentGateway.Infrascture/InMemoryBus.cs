using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleCQRS;

namespace PaymentGateway.Infrastructure
{
    public class InMemoryBus : ICommandSender, IEventPublisher
    {
        private readonly Dictionary<Type, List<Func<Message, Task>>> _asyncRoutes = new Dictionary<Type, List<Func<Message, Task>>>();

        public void RegisterHandler<T>(Func<T, Task> handler) where T : Message
        {
            if(!_asyncRoutes.TryGetValue(typeof(T), out var handlers))
            {
                handlers = new List<Func<Message, Task>>();
                _asyncRoutes.Add(typeof(T), handlers);
            }

            handlers.Add((x => handler((T)x)));
        }

        public void Send<T>(T command) where T : Command
        {
            if (_asyncRoutes.TryGetValue(typeof(T), out var handlers))
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
            if (!_asyncRoutes.TryGetValue(@event.GetType(), out var asyncHandlers))
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
        Task Publish<T>(T @event) where T : Event;
    }
}
