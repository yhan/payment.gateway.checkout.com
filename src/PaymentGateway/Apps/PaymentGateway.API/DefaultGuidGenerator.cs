using System;

namespace PaymentGateway.API
{
    public class DefaultGuidGenerator : IGenerateGuid
    {
        public Guid Generate()
        {
            return Guid.NewGuid();
        }
    }
}