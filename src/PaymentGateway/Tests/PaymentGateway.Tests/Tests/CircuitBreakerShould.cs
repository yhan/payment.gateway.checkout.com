using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class CircuitBreakerShould
    {
        [Test]
        public async Task Should_open_circuit()
        {
            // Break the circuit after the specified number of consecutive exceptions
            // and keep circuit broken for the specified duration,
            // calling an action on change of circuit state,
            // passing a context provided to Execute().

            bool stopCall = false;

            Action<Exception, TimeSpan, Context> onBreak = (exception, timespan, context) =>
            {
                stopCall = true;
                Console.WriteLine("broken");
            };

            Action<Context> onReset = context =>
            {
                stopCall = false;
                Console.WriteLine("Reset");
            };

            AsyncCircuitBreakerPolicy breaker = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking: 2, 
                                    durationOfBreak: TimeSpan.FromMilliseconds(20), 
                                    onBreak: onBreak, 
                                    onReset: onReset);

            var policy = Policy
                .Handle<Exception>()
                .FallbackAsync(cancel => FallBack())
                .WrapAsync(breaker);


            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(5);
                await policy.ExecuteAsync(async () =>
                {
                    if(!stopCall)
                    {
                        TestContext.WriteLine(await CallExternal());
                    }
                });
            }
        }

        private Task FallBack()
        {
            Console.WriteLine("fall back called");
            return Task.CompletedTask;
        }


        private async Task<string> CallExternal()
        {
            return await External("hello");
        }

        private int _failCount = 0;

        private Task<string> External(string received)
        {
            if(_failCount < 3)
            {
                _failCount++;
                throw new Exception();
            }
            return Task.FromResult(received);
        }
    }


    [TestFixture]
    public class PollyRetryShould
    {
        [Test]
        public void Should_retry_until_threshold_is_reached()
        {
            AsyncCircuitBreakerPolicy breaker = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(3,
                    TimeSpan.FromMilliseconds(1000),
                    OnBreak, OnReset);

            AsyncPolicyWrap retryPolicy = Policy.Handle<Exception>().RetryAsync(3)
                .WrapAsync(breaker);



            //var policy = Policy
            //    .Handle<TaskCanceledException>()
            //    .Or<FailedConnectionToBankException>()
            //    .FallbackAsync(cancel => ReturnWillHandleLater(payment.GatewayPaymentId, payment.RequestId))
            //    .WrapAsync(breaker);
            
            
            
            int count = 0;



            retryPolicy.ExecuteAsync(async () =>
            {
                Console.WriteLine(count++);
                await Task.FromException(new Exception());

            });
        }

        private void OnReset(Context obj)
        {
            throw new NotImplementedException();
        }

        private void OnBreak(Exception arg1, TimeSpan arg2, Context arg3)
        {
            throw new NotImplementedException();
        }
    }
    
}