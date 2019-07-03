

namespace PaymentGateway.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using AcquiringBanks.Stub;
    using Domain;

    /// <inheritdoc cref="IProcessPayment"/>>
    public class PaymentProcessor : IProcessPayment
    {
        private readonly IMapMerchantToBankAdapter _bankAdapterMapper;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;
        private readonly SimulateGatewayException _gatewayExceptionSimulator;

        public PaymentProcessor(IMapMerchantToBankAdapter bankAdapterMapper, IEventSourcedRepository<Payment> paymentsRepository, SimulateGatewayException gatewayExceptionSimulator = null)
        {
            _bankAdapterMapper = bankAdapterMapper;
            _paymentsRepository = paymentsRepository;
            _gatewayExceptionSimulator = gatewayExceptionSimulator;
        }

        public async Task AttemptPaying(IAdaptToBank bankAdapter, PayingAttempt payingAttempt)
        {
            var bankResponse = await bankAdapter.RespondToPaymentAttempt(payingAttempt);

            IHandleBankResponseStrategy strategy = Build(bankResponse, _paymentsRepository);

            await strategy.Handle(_gatewayExceptionSimulator, payingAttempt.GatewayPaymentId);
        }

        private static IHandleBankResponseStrategy Build(IBankResponse bankResponse, IEventSourcedRepository<Payment> paymentsRepository)
        {
            switch (bankResponse)
            {
                case BankResponse response:
                    return new RespondedBankStrategy(response, paymentsRepository);

                case BankDoesNotRespond _:
                    return new NotRespondedBankStrategy(paymentsRepository);
            }

            throw new ArgumentException();
        }
    }
}