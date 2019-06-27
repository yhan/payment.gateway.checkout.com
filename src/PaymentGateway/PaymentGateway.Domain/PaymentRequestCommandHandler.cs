using System.Threading;
using System.Threading.Tasks;
using SimpleCQRS;

namespace PaymentGateway.Domain
{
    public class PaymentRequestCommandHandler : ICommandHandler<RequestPaymentCommand>
    {
        private readonly IProvidePaymentIdsMapping _paymentIdsMapping;
        private readonly IProcessPayment _acquiringBank;
        private readonly IEventSourcedRepository<Payment> _repository;

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

            //BankResponse bankResponse = await acquringBank.Pay(payment);

            await Task.Run(() => { _acquiringBank.AttemptPaying(payment); }); //TODO: Add cancellation with a timeout
            //await  _acquiringBank.AttemptPaying(payment); 


            return this.Success(payment);
        }
    }
}