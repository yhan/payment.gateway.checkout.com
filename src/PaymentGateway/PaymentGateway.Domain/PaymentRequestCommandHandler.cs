using System;
using System.Threading.Tasks;
using SimpleCQRS;

namespace PaymentGateway.Domain
{

    public static class Stream
    {
        public const int NotCreatedYet = -1;
    }

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
            _repository.Save(payment, Stream.NotCreatedYet);
        }
    }

    public class PaymentRequested : Event
    {
        public Guid GatewayPaymentId { get; }
        public Guid RequestId { get; }
        public CreditCard CreditCard { get; }
        public Money Amount { get; }

        public PaymentRequested(Guid gatewayPaymentId, Guid requestId, CreditCard creditCard, Money amount)
        {
            GatewayPaymentId = gatewayPaymentId;
            RequestId = requestId;
            CreditCard = creditCard;
            Amount = amount;
        }
    }
}