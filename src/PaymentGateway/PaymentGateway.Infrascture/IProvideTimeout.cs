using System;

namespace PaymentGateway.Infrastructure
{
    public interface IProvideTimeout
    {
        TimeSpan GetTimeout();
    }
}