using System;
using System.Diagnostics;

namespace PaymentGateway.Domain
{
    [DebuggerDisplay("{Value}")]
    public struct PaymentRequestId
    {
        public Guid Value { get; }

        public PaymentRequestId(Guid value)
        {
            Value = value;
        }
    }
}