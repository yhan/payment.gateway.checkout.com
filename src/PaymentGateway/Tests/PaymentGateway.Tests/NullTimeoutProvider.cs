using System;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.Tests
{
    public class NullTimeoutProvider : IProvideTimeout
    {
        public TimeSpan GetTimeout()
        {
            return TimeSpan.Zero;
        }
    }
}