using System;

namespace PaymentGateway
{
    public class DefaultGuidGenerator : IGenerateGuid
    {
        public Guid Generate()
        {
            return Guid.NewGuid();
        }
    }
}