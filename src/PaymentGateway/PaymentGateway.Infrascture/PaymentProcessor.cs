using System;
using System.Threading;
using System.Threading.Tasks;
using PaymentGateway.Domain;
using PaymentGateway.Domain.AcquiringBank;
using SimpleCQRS;

namespace PaymentGateway.Infrastructure
{
    public class PaymentProcessor : IProcessPayment
    {
        private readonly ITalkToAcquiringBank _acquiringBank;
        private readonly IEventSourcedRepository<Payment> _paymentsRepository;
        private readonly SimulateException _exceptionSimulator;

        public PaymentProcessor(ITalkToAcquiringBank acquiringBank, IEventSourcedRepository<Payment> paymentsRepository, SimulateException exceptionSimulator = null)
        {
            _acquiringBank = acquiringBank;
            _paymentsRepository = paymentsRepository;
            _exceptionSimulator = exceptionSimulator;
        }

        public async Task AttemptPaying(PayingAttempt payingAttempt)
        {
            Payment knownPayment = null;
            Guid bankPaymentId = Guid.Empty;

            try
            {
                var bankResponse = await _acquiringBank.Pay(payingAttempt);
                bankPaymentId = bankResponse.BankPaymentId;
                try
                {
                    knownPayment = await _paymentsRepository.GetById(payingAttempt.GatewayPaymentId);
                    _exceptionSimulator?.Throws();
                }
                catch (AggregateNotFoundException)
                {
                    //TODO : log
                    return;
                }

                switch (bankResponse.PaymentStatus)
                {
                    case BankPaymentStatus.Accepted:
                        knownPayment.AcceptPayment(bankPaymentId);
                        break;
                    case BankPaymentStatus.Rejected:
                        knownPayment.BankRejectPayment(bankPaymentId);
                        break;
                }

                await _paymentsRepository.Save(knownPayment, knownPayment.Version);
            }
           
            catch (Exception)
            {
                //TODO: log
                knownPayment.FailOnGateway(bankPaymentId);
                await _paymentsRepository.Save(knownPayment, knownPayment.Version);
            }
        }
    }

    public class SimulateException
    {
        public void Throws()
        {
            throw new FakeException();
        }

        private class FakeException : Exception{}
    }
}