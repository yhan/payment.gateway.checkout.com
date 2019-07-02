using System;
using AcquiringBanks.API;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public interface ISelectAdapter
    {
        IAdaptToBank Select(Bank bank);
    }

    public class BankAdapterSelector : ISelectAdapter
    {
        private readonly IRandomnizeAcquiringBankPaymentStatus _random;
        private readonly IGenerateBankPaymentId _bankPaymentIdGenerator;
        private readonly IProvideRandomBankResponseTime _delayProvider;
        private readonly IConnectToAcquiringBanks _connectionBehavior;
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;
        private readonly ILogger<BankAdapterSelector> _logger;

        public BankAdapterSelector(IRandomnizeAcquiringBankPaymentStatus random, 
            IGenerateBankPaymentId bankPaymentIdGenerator, 
            IProvideRandomBankResponseTime delayProvider,
            IConnectToAcquiringBanks connectionBehavior,
            IMapAcquiringBankToPaymentGateway paymentIdsMapper,
            ILogger<BankAdapterSelector> logger )
        {
            _random = random;
            _bankPaymentIdGenerator = bankPaymentIdGenerator;
            _delayProvider = delayProvider;
            _connectionBehavior = connectionBehavior;
            _paymentIdsMapper = paymentIdsMapper;
            _logger = logger;
            
        }

        public IAdaptToBank Select(Bank bank)
        {
            switch (bank)
            {
                case Bank.SocieteGenerale:
                    return new SoiceteGeneraleAdapter(_random, _bankPaymentIdGenerator, _delayProvider, _connectionBehavior, _paymentIdsMapper, _logger);
                case Bank.BNP:
                    return new BNPAdapter(_random, _bankPaymentIdGenerator, _delayProvider, _connectionBehavior, _paymentIdsMapper, _logger);
                default:
                    throw new ArgumentOutOfRangeException(nameof(bank), bank, null);
            }
        }
    }
}