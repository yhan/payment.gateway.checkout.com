using System;
using AcquiringBanks.Stub;
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
        private readonly IRandomnizeAcquiringBankPaymentStatus _paymentStatusRandom;
        private readonly IGenerateBankPaymentId _bankPaymentIdGenerator;
        private readonly IProvideRandomBankResponseTime _delayProvider;
        private readonly IConnectToAcquiringBanks _connectionBehavior;
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;
        private readonly ILogger<BankAdapterSelector> _logger;

        public BankAdapterSelector(IRandomnizeAcquiringBankPaymentStatus paymentStatusRandom, 
            IGenerateBankPaymentId bankPaymentIdGenerator, 
            IProvideRandomBankResponseTime delayProvider,
            IConnectToAcquiringBanks connectionBehavior,
            IMapAcquiringBankToPaymentGateway paymentIdsMapper,
            ILogger<BankAdapterSelector> logger )
        {
            _paymentStatusRandom = paymentStatusRandom;
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
                    return new SoiceteGeneraleAdapter(_delayProvider, _connectionBehavior, 
                        _paymentIdsMapper, new SocieteGenerale(_bankPaymentIdGenerator, _paymentStatusRandom, _connectionBehavior), 
                        _logger);
                case Bank.BNP:
                    return new BNPAdapter(_delayProvider, _connectionBehavior, _paymentIdsMapper, new BNP(_bankPaymentIdGenerator, _paymentStatusRandom, _connectionBehavior), _logger);
                default:
                    throw new ArgumentOutOfRangeException(nameof(bank), bank, null);
            }
        }
    }
}