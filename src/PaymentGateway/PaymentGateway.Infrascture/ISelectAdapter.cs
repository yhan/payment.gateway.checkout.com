using System;
using AcquiringBanks.Stub;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    /// <summary>
    /// Select the proper Bank Adapter
    /// </summary>
    public interface ISelectAdapter
    {
        IAdaptToBank Select(Bank bank);
    }

    /// <inheritdoc cref="ISelectAdapter"/>
    public class BankAdapterSelector : ISelectAdapter
    {
        private readonly IGenerateAcquiringBankPaymentStatus _paymentStatusRandom;
        private readonly IGenerateBankPaymentId _bankPaymentIdGenerator;
        private readonly IProvideBankResponseTime _delayProvider;
        private readonly IConnectToAcquiringBanks _connectionBehavior;
        private readonly IMapAcquiringBankToPaymentGateway _paymentIdsMapper;
        private readonly ILogger<BankAdapterSelector> _logger;

        public BankAdapterSelector(IGenerateAcquiringBankPaymentStatus paymentStatusRandom, 
            IGenerateBankPaymentId bankPaymentIdGenerator, 
            IProvideBankResponseTime delayProvider,
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
                    return new SoiceteGeneraleAdapter(_delayProvider, 
                        _paymentIdsMapper, new SocieteGenerale(_bankPaymentIdGenerator, _paymentStatusRandom, _connectionBehavior), 
                        _logger);

                case Bank.BNP:
                    return new BNPAdapter(_delayProvider, _paymentIdsMapper, new BNP(_bankPaymentIdGenerator, _paymentStatusRandom, _connectionBehavior), _logger);


                case Bank.StupidBankForDemo:
                    return new StupidBankAlwaysSendTheSamePaymentId(_delayProvider, _paymentIdsMapper, _logger);

                default:
                    throw new ArgumentOutOfRangeException(nameof(bank), bank, null);
            }
        }
    }
}