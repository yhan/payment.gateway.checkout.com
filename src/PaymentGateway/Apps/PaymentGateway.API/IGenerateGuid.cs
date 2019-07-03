using System;

namespace PaymentGateway
{
    /// <summary>
    /// Generate Unique Identifier
    /// </summary>
    public interface IGenerateGuid
    {
        Guid Generate();
    }
}