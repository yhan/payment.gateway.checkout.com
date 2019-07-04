using System;
using System.Threading;
using System.Threading.Tasks;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using Polly;

namespace PaymentGateway.Infrastructure
{
    public abstract class AdaptToBank : IAdaptToBank
    {
        protected readonly IProvideBankResponseTime DelayProvider;
        protected readonly ILogger<BankAdapterSelector> Logger;

        protected AdaptToBank(IProvideBankResponseTime delayProvider, ILogger<BankAdapterSelector> logger)
        {
            DelayProvider = delayProvider;
            Logger = logger;
        }

        public async Task<IBankResponse> RespondToPaymentAttempt(PayingAttempt paymentAttempt, CancellationToken cancellationToken)
        {
            // Connection to bank
            var policy = Policy.Handle<FailedConnectionToBankException>()
                .WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(Math.Pow(2, retry)));

            var policyResult = await policy.ExecuteAndCaptureAsync(async () => await Connect());
            if (!policyResult.Result)
            {
                return new BankDoesNotRespond(paymentAttempt.GatewayPaymentId);
            }
            
            return await CallBank(paymentAttempt, cancellationToken);
        }

        public abstract Task<bool> Connect();

        protected abstract Task<IBankResponse> CallBank(PayingAttempt payingAttempt, CancellationToken cancellationToken);
    }
}