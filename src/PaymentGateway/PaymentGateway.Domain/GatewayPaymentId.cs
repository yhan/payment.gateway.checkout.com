using System;

namespace PaymentGateway.Domain
{
    public struct GatewayPaymentId
    {
        public Guid Value { get; }

        public GatewayPaymentId(Guid value)
        {
            Value = value;
        }
    }
}