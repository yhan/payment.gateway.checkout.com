using System.Threading.Tasks;
using PaymentGateway.Domain;
using PaymentGateway.Domain.AcquiringBank;
using SimpleCQRS;

namespace PaymentGateway.Infrastructure
{
    public class AcquiringBanksMediator : IProcessPayment
    {
        private readonly ITalkToAcquiringBank _acquiringBank;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;

        public AcquiringBanksMediator(ITalkToAcquiringBank acquiringBank, IEventSourcedRepository<Payment> paymentsRepository)
        {
            _acquiringBank = acquiringBank;
            _paymentsRepository = paymentsRepository;
        }

        public async Task AttemptPaying(PayingAttempt payingAttempt)
        {
            var bankResponse = await _acquiringBank.Pay(payingAttempt);
            var knownPayment = await _paymentsRepository.GetById(payingAttempt.GatewayPaymentId);

            switch (bankResponse.PaymentStatus)
            {
                case BankPaymentStatus.Accepted:
                    knownPayment.AcceptPayment(bankResponse.BankPaymentId);
                    break;
                case BankPaymentStatus.Rejected:
                    knownPayment.RejectPayment(bankResponse.BankPaymentId);
                    break;
            }

            await _paymentsRepository.Save(knownPayment, knownPayment.Version);
        }
    }
}