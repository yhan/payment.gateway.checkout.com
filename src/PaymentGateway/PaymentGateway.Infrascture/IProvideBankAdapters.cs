using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    /// <inheritdoc cref="IMapAcquiringBankToPaymentGateway"/>
    public class MerchantToBankAdapterMapper : IMapMerchantToBankAdapter
    {
        public MerchantToBankAdapterMapper(ISelectAdapter adapterSelector)
        {
            _store.Add(MerchantsRepository.Amazon, adapterSelector.Select(Bank.SocieteGenerale));
            _store.Add(MerchantsRepository.Apple, adapterSelector.Select(Bank.BNP));
        }

        private readonly IDictionary<Guid, IAdaptToBank> _store = new Dictionary<Guid, IAdaptToBank>();

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
}