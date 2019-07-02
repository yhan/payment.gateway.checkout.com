using System;
using AcquiringBanks.API;

namespace PaymentGateway.Infrastructure
{
    public class BankDoesNotRespond : IBankResponse
    {
        public Guid GatewayPaymentId { get; }

        public BankDoesNotRespond(Guid gatewayPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
        }
    }
}