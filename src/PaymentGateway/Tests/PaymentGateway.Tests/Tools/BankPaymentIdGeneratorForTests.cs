using System;
using AcquiringBanks.Stub;

namespace PaymentGateway.Tests
{
    class BankPaymentIdGeneratorForTests : IGenerateBankPaymentId
    {
        private readonly Guid _value;

        public BankPaymentIdGeneratorForTests(Guid value)
        {
            _value = (value);
        }
        public Guid Generate()
        {
            return _value;
        }
    }
}