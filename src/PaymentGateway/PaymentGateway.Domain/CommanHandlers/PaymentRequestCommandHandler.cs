using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

[assembly:InternalsVisibleTo("PaymentGateway.Tests")]

namespace PaymentGateway.Domain
{
    public class PaymentRequestCommandHandler : ICommandHandler<RequestPaymentCommand>
    {
        private readonly IKnowAllPaymentRequests _paymentRequestsMemory;
        private readonly IProcessPayment _paymentProcessor;
        private readonly IMapMerchantToBankAdapter _bankAdapterMapper;
        private readonly IKnowSendRequestToBankSynchrony _synchronyMaster;
        private readonly ILogger<PaymentRequestCommandHandler> _logger;
        private readonly IEventSourcedRepository<Payment> _repository;
        
        public PaymentRequestCommandHandler(IEventSourcedRepository<Payment> repository,
            IKnowAllPaymentRequests paymentRequestsMemory,
            IProcessPayment paymentProcessor,
            IMapMerchantToBankAdapter bankAdapterMapper, 
            IKnowSendRequestToBankSynchrony synchronyMaster, ILogger<PaymentRequestCommandHandler> logger)
        {
            _repository = repository;
            _paymentRequestsMemory = paymentRequestsMemory;
            _paymentProcessor = paymentProcessor;
            _bankAdapterMapper = bankAdapterMapper;
            _synchronyMaster = synchronyMaster;
            _logger = logger;
        }

        public async Task<ICommandResult> Handle(RequestPaymentCommand command)
        {
            Payment payment;
            try
            {
                var paymentRequestId = new PaymentRequestId(command.RequestId);
                if (await _paymentRequestsMemory.AlreadyHandled(paymentRequestId))
                {
                    //payment request already handled
                    return this.Invalid(command.RequestId, "Identical payment request will not be handled more than once");
                }

                var bankAdapter = _bankAdapterMapper.FindBankAdapter(command.MerchantId);

                payment = new Payment(command.GatewayPaymentId, command.MerchantId, command.RequestId, command.Card, command.Amount);
                await _repository.Save(payment, Stream.NotCreatedYet);
                await _paymentRequestsMemory.Remember(paymentRequestId);


                //TODO: Add cancellation with a timeout
                if(_synchronyMaster.SendPaymentRequestAsynchronously())
                {
                    _paymentProcessor.AttemptPaying(bankAdapter, payment).ContinueWith((task, o) =>
                    {
                        _logger.LogError($"Payment request '{command.RequestId}' with exception {task.Exception?.Message}");
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
                else
                {
                    var paymentResult =  await _paymentProcessor.AttemptPaying(bankAdapter, payment);
                    if (paymentResult.Status == PaymentStatus.Timeout)
                    {
                        return this.Failure($"{paymentResult.Identifier} Timeout ");
                    }
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
                                     paymentRequestId: payment.RequestId, 
                                     cardNumber: payment.Card.Number, 
                                     cardCvv: payment.Card.Cvv,
                                     cardExpiry: payment.Card.Expiry,
                                     amount: payment.Amount.Value, 
                                     currency: payment.Amount.Currency);
        }
    }
}