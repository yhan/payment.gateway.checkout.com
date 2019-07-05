using System;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure
{
    internal class NullBankResponseHandler : IHandleBankResponseStrategy
    {
        public Task Handle(IThrowsException gatewayExceptionSimulator, Guid payingAttemptGatewayPaymentId)
        {
            return Task.CompletedTask;
        }
    }
}