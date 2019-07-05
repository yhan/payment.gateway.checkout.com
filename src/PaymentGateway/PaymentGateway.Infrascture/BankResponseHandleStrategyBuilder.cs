using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class BankResponseHandleStrategyBuilder
    {
        public static IHandleBankResponseStrategy Build(IBankResponse bankResponse, IEventSourcedRepository<Payment> paymentsRepository)
        {
            switch (bankResponse)
            {
                case BankResponse response:
                    return new RespondedBankStrategy(response, paymentsRepository);

                case BankDoesNotRespond _:
                    return new NotRespondedBankStrategy(paymentsRepository);

                case NullBankResponse _:
                    return new NullBankResponseHandler();
            }

            throw new ArgumentException();
        }
    }
}