using System;
using System.Collections.Generic;

namespace PaymentGateway.Infrastructure
{
    public interface IMapMerchantToBankAdapter
    {
        IAdaptToBank FindBankAdapter(Guid merchantId);
    }

    public class MerchantToBankAdapterMapper : IMapMerchantToBankAdapter
    {
        public MerchantToBankAdapterMapper(ISelectAdapter adapterSelector)
        {
            _store.Add(Amazon, adapterSelector.Select(Bank.SocieteGenerale));
            _store.Add(Apple, adapterSelector.Select(Bank.BNP));
        }

        private readonly IDictionary<Guid, IAdaptToBank> _store = new Dictionary<Guid, IAdaptToBank>();
        public static Guid Amazon = Guid.Parse("2d0ae468-7ac9-48f4-be3f-73628de3600e");
        public static Guid Apple= Guid.Parse("06c6116f-1d4e-44d3-ae9f-8df90f991a52");

        /// <summary>
        /// Simulate a merchant's id to bank adapter.
        /// Prerequisite is to onboard each merchant.
        /// During the onboarding, `Gateway` should be aware of "to which bank should I route a payment request.
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        
        public IAdaptToBank FindBankAdapter(Guid merchantId)
        {
            if (_store.TryGetValue(merchantId, out var adapter))
            {
                return adapter;
            }

            throw new BankOnboardMissingException(merchantId);
        }
    }

    public enum Bank
    {
        SocieteGenerale,
        BNP
    }

    public class BankOnboardMissingException : Exception
    {
        public BankOnboardMissingException(Guid merchantId): base($"Merchant {merchantId} has not been onboarded")
        {
            
        }
    }
}