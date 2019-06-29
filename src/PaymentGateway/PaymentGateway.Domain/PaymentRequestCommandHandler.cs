using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PaymentGateway.Domain.AcquiringBank;
using SimpleCQRS;

[assembly:InternalsVisibleTo("PaymentGateway.Tests")]

namespace PaymentGateway.Domain
{
    public class PaymentRequestCommandHandler : ICommandHandler<RequestPaymentCommand>
    {
        private readonly IProvidePaymentIdsMapping _paymentIdsMapping;
        private readonly IProcessPayment _acquiringBank;
        private readonly IEventSourcedRepository<Payment> _repository;

        private ManualResetEventSlim _wait = new ManualResetEventSlim(false, 100);

        public PaymentRequestCommandHandler(IEventSourcedRepository<Payment> repository,
            IProvidePaymentIdsMapping paymentIdsMapping,
            IProcessPayment acquiringBank)
        {
            _repository = repository;
            _paymentIdsMapping = paymentIdsMapping;
            _acquiringBank = acquiringBank;
        }

        public async Task<ICommandResult> Handle(RequestPaymentCommand command)
        {
            if (await _paymentIdsMapping.AlreadyHandled(command.RequestId))
            {
                //payment request already handled
                return this.Invalid("Identical payment request will not be handled more than once");
            }

            var payment = new Payment(command.GatewayPaymentId, command.RequestId, command.CreditCard, command.Amount);
            await _repository.Save(payment, Stream.NotCreatedYet);
            await _paymentIdsMapping.Remember(command.RequestId);

            //TODO: Add cancellation with a timeout
            _acquiringBank.AttemptPaying(payment.MapToAcquiringBank())
                .ContinueWith( t =>
                    {
                        _wait.Set();
                    }
                    );

            return this.Success(payment);
        }

        internal void Wait()
        {
            _wait.Wait();
        }
    }

    public static class PaymentExtensions
    {
        public static PayingAttempt MapToAcquiringBank(this Payment payment)
        {
            return new PayingAttempt(payment.GatewayPaymentId, payment.CreditCard.Number, payment.CreditCard.Cvv,
                payment.CreditCard.Expiry, payment.CreditCard.HolderName, payment.Amount.Amount,
                payment.Amount.Currency);
        }
    }
}