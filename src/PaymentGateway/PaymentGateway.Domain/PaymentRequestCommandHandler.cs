using System;
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
        private readonly IKnowAllPaymentRequests _paymentRequests;
        private readonly IProcessPayment _acquiringBank;
        private readonly bool _asynchronous;
        private readonly IEventSourcedRepository<Payment> _repository;
        
        public PaymentRequestCommandHandler(IEventSourcedRepository<Payment> repository,
            IKnowAllPaymentRequests paymentRequests,
            IProcessPayment acquiringBank, bool asynchronous)
        {
            _repository = repository;
            _paymentRequests = paymentRequests;
            _acquiringBank = acquiringBank;
            _asynchronous = asynchronous;
        }

        public async Task<ICommandResult> Handle(RequestPaymentCommand command)
        {
            var paymentRequestId = new PaymentRequestId(command.RequestId);
            if (await _paymentRequests.AlreadyHandled(paymentRequestId))
            {
                //payment request already handled
                return this.Invalid("Identical payment request will not be handled more than once");
            }

            var payment = new Payment(command.GatewayPaymentId, command.MerchantId, command.RequestId, command.CreditCard, command.Amount);
            await _repository.Save(payment, Stream.NotCreatedYet);
            await _paymentRequests.Remember(paymentRequestId);

            //TODO: Add cancellation with a timeout
            if(_asynchronous)
            {
                _acquiringBank.AttemptPaying(payment.MapToAcquiringBank());
            }
            else
            {
                await _acquiringBank.AttemptPaying(payment.MapToAcquiringBank());
            }

            return this.Success(payment);
        }

       
    }


    public static class PaymentExtensions
    {
        public static PayingAttempt MapToAcquiringBank(this Payment payment)
        {
            return new PayingAttempt(payment.GatewayPaymentId, payment.MerchantId, payment.CreditCard.Number, payment.CreditCard.Cvv,
                payment.CreditCard.Expiry, payment.CreditCard.HolderName, payment.Amount.Amount,
                payment.Amount.Currency);
        }
    }
}