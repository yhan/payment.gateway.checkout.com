using System.Threading.Tasks;
using PaymentGateway.Domain;
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

        public async Task AttemptPaying(Payment payment /*TODO maybe introduce PayingAttempt*/)
        {
            var bankResponse = await _acquiringBank.Pay(payment);

            var knownPayment = await _paymentsRepository.GetById(payment.GatewayPaymentId);

            knownPayment.AcceptPayment(bankResponse);

            await _paymentsRepository.Save(knownPayment, knownPayment.Version);
        }
    }

   
}