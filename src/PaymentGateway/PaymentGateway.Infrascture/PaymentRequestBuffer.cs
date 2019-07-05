using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    internal class PaymentRequestBuffer
    {
        public IAdaptToBank BankAdapter { get; }
        public PayingAttempt PayingAttempt { get; }
        public Payment Payment { get; }

        public PaymentRequestBuffer(IAdaptToBank bankAdapter, PayingAttempt payingAttempt, Payment payment)
        {
            BankAdapter = bankAdapter;
            PayingAttempt = payingAttempt;
            Payment = payment;
        }
    }
}