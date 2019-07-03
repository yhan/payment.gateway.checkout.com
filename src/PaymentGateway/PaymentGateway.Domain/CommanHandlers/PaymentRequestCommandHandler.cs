using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("PaymentGateway.Tests")]

namespace PaymentGateway.Domain
{
    public class PaymentRequestCommandHandler : ICommandHandler<RequestPaymentCommand>
    {
        private readonly IKnowAllPaymentRequests _paymentRequests;
        private readonly IProcessPayment _paymentProcessor;
        private readonly IMapMerchantToBankAdapter _bankAdapterMapper;
        private readonly bool _asynchronous;
        private readonly IEventSourcedRepository<Payment> _repository;
        
        public PaymentRequestCommandHandler(IEventSourcedRepository<Payment> repository,
            IKnowAllPaymentRequests paymentRequests,
            IProcessPayment paymentProcessor, IMapMerchantToBankAdapter bankAdapterMapper, bool asynchronous)
        {
            _repository = repository;
            _paymentRequests = paymentRequests;
            _paymentProcessor = paymentProcessor;
            _bankAdapterMapper = bankAdapterMapper;
            _asynchronous = asynchronous;
        }

        public async Task<ICommandResult> Handle(RequestPaymentCommand command)
        {
            Payment payment = null;
            try
            {
                var paymentRequestId = new PaymentRequestId(command.RequestId);
                if (await _paymentRequests.AlreadyHandled(paymentRequestId))
                {
                    //payment request already handled
                    return this.Invalid(command.RequestId, "Identical payment request will not be handled more than once");
                }

                payment = new Payment(command.GatewayPaymentId, command.MerchantId, command.RequestId, command.Card, command.Amount);
                await _repository.Save(payment, Stream.NotCreatedYet);
                await _paymentRequests.Remember(paymentRequestId);

                var bankAdapter = _bankAdapterMapper.FindBankAdapter(command.MerchantId);

                //TODO: Add cancellation with a timeout
                if(_asynchronous)
                {
                    _paymentProcessor.AttemptPaying(bankAdapter, payment.MapToAcquiringBank()).ContinueWith((task, o) =>
                    {
                        //return this.Invalid(command.RequestId, task.Exception?.Message);

                        //log errors


                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
                else
                {
                    await _paymentProcessor.AttemptPaying(bankAdapter, payment.MapToAcquiringBank());
                }
            }
            catch (BankOnboardMissingException e)
            {
                return this.Invalid(command.RequestId, e.Message);
            }

            return this.Success(payment);
        }

       
    }


    public static class PaymentExtensions
    {
        public static PayingAttempt MapToAcquiringBank(this Payment payment)
        {
            return new PayingAttempt(gatewayPaymentId: payment.GatewayPaymentId, 
                                     merchantId: payment.MerchantId, 
                                     cardNumber: payment.Card.Number, 
                                     cardCvv: payment.Card.Cvv,
                                     cardExpiry: payment.Card.Expiry,
                                     amount: payment.Amount.Value, 
                                     currency: payment.Amount.Currency);
        }
    }
}