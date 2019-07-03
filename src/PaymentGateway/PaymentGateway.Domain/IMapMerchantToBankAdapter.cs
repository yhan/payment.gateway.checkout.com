using System;

namespace PaymentGateway.Domain
{
    /// <summary>
    /// Deduce bank adapter from merchant's id
    /// </summary>
    public interface IMapMerchantToBankAdapter
    {
        IAdaptToBank FindBankAdapter(Guid merchantId);
    }
}