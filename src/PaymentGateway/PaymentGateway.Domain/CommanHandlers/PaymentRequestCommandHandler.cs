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
        private readonly IKnowSendRequestToBankSynchrony _synchronyMaster;
        private readonly IEventSourcedRepository<Payment> _repository;
        
        public PaymentRequestCommandHandler(IEventSourcedRepository<Payment> repository,
            IKnowAllPaymentRequests paymentRequests,
            IProcessPayment paymentProcessor,
            IMapMerchantToBankAdapter bankAdapterMapper, 
            IKnowSendRequestToBankSynchrony synchronyMaster)
        {
            _repository = repository;
            _paymentRequests = paymentRequests;
            _paymentProcessor = paymentProcessor;
            _bankAdapterMapper = bankAdapterMapper;
            _synchronyMaster = synchronyMaster;
        }

        public async Task<ICommandResult> Handle(RequestPaymentCommand command)
        {
            Payment payment;
            try
            {
                var paymentRequestId = new PaymentRequestId(command.RequestId);
                if (await _paymentRequests.AlreadyHandled(paymentRequestId))
                {
                    //payment request already handled
                    return this.Invalid(command.RequestId, "Identical payment request will not be handled more than once");
                }

                var bankAdapter = _bankAdapterMapper.FindBankAdapter(command.MerchantId);

                payment = new Payment(command.GatewayPaymentId, command.MerchantId, command.RequestId, command.Card, command.Amount);
                await _repository.Save(payment, Stream.NotCreatedYet);
                await _paymentRequests.Remember(paymentRequestId);


                //TODO: Add cancellation with a timeout
                if(_synchronyMaster.SendPaymentRequestAsynchronously())
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