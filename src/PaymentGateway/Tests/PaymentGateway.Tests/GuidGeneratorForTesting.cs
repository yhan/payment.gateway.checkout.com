using System;

namespace PaymentGateway.Tests
{
    /// <summary>
    /// Deterministic Unique Identifier generation for tests purpose
    /// </summary>
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