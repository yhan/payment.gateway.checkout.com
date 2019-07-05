using System;
using System.Collections.Concurrent;
using Polly.CircuitBreaker;
using Polly.Wrap;

namespace PaymentGateway.Infrastructure
{
    public interface IAmCircuitBreakers
    {
        bool TryGet(Type bankAdapterType, out CircuitBreaker circuitBreaker);
        void Add(Type bankAdapterType, CircuitBreaker circuitBreaker);
    }

    public class CircuitBreaker
    {
        public AsyncCircuitBreakerPolicy Breaker { get; }
        public AsyncPolicyWrap Policy { get; }

        public CircuitBreaker(AsyncCircuitBreakerPolicy breaker, AsyncPolicyWrap policy)
        {
            Breaker = breaker;
            Policy = policy;
        }

        public void Reset()
        {
            Breaker.Reset();
        }
    }

    public class CircuitBreakerRepository : IAmCircuitBreakers
    {
        private readonly ConcurrentDictionary<Type, CircuitBreaker> _map = new ConcurrentDictionary<Type, CircuitBreaker>();

        public bool TryGet(Type bankAdapterType, out CircuitBreaker circuitBreaker)
        {

            if(_map.TryGetValue(bankAdapterType, out var breaker ))
            {
                circuitBreaker = breaker;
                return true;
            }

            circuitBreaker = null;
            return false;
        }

        public void Add(Type bankAdapterType, CircuitBreaker circuitBreaker)
        {
            _map.TryAdd(bankAdapterType, circuitBreaker);
        }
    }
}