using System;

namespace PaymentGateway.Domain
{
    public struct PaymentRequestId
    {
        public Guid Value { get; }

        public PaymentRequestId(Guid value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}