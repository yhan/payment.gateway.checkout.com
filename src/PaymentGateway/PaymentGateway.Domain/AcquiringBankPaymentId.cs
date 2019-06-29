using System;

namespace PaymentGateway.Domain
{
    public struct AcquiringBankPaymentId
    {
        public Guid Value { get; }

        public AcquiringBankPaymentId(Guid value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}