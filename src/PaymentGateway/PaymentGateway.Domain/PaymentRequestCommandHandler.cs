using System.Threading.Tasks;
using SimpleCQRS;

namespace PaymentGateway.Domain
{
    public class PaymentRequestCommandHandler
    {
        private readonly IEventSourcedRepository<Payment> _repository;

        public PaymentRequestCommandHandler(IEventSourcedRepository<Payment> repository)
        {
            _repository = repository;
        }

        public async Task Handle(RequestPaymentCommand command)
        {
            var payment = new Payment(command.GatewayPaymentId, command.RequestId, command.CreditCard, command.Amount);
            await _repository.Save(payment, Stream.NotCreatedYet);
        }
    }
}