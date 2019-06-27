using System;
using PaymentGateway.API;

namespace PaymentGateway.Tests
{
    public class GuidGeneratorForTesting : IGenerateGuid
    {
        public Guid Guid { get; }

        public GuidGeneratorForTesting(Guid guid)
        {
            Guid = guid;
        }

        public Guid Generate()
        {
            return Guid;
        }
    }
}