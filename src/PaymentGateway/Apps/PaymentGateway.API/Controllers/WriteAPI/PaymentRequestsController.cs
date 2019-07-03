using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using PaymentGateway.ReadAPI;

namespace PaymentGateway.WriteAPI
{
    [Route("api/Payments")]
    [ApiController]
    public class PaymentRequestsController : ControllerBase
    {
        private readonly ICommandHandler<RequestPaymentCommand> _commandHandler;
 

        public PaymentRequestsController(ICommandHandler<RequestPaymentCommand> commandHandler, ILogger<PaymentRequestsController> logger)
        {
            _commandHandler = commandHandler;
        }

        [HttpPost]
        public async Task<IActionResult> ProceedPaymentRequest([FromBody]PaymentRequest paymentRequest, 
            [FromServices]IGenerateGuid gatewayPaymentIdGenerator,
            [FromServices]IKnowAllPaymentRequests paymentRequestsRepository,
            [FromServices]IProcessPayment paymentProcessor )
        {
            if (ReturnBadRequestWhenReceivedInvalidPaymentRequest(paymentRequest, out var actionResult))
            {
                return actionResult;
            }

            var gatewayPaymentId = gatewayPaymentIdGenerator.Generate();

            var commandResult = await _commandHandler.Handle(paymentRequest.AsCommand(gatewayPaymentId));
            switch (commandResult)
            {
                case SuccessCommandResult<Payment> success:
                    var paymentDto = success.Entity.AsDto();

                    return AcceptedAtRoute(nameof(PaymentReadController.GetPaymentInfo),
                        routeValues: new {gateWayPaymentId = paymentDto.GatewayPaymentId},
                        value: paymentDto);

                case InvalidCommandResult invalid:
                    return ActionResultHelper.ToActionResult(invalid);

                default:
                    throw new NotSupportedException();
            }
        }

        private static bool ReturnBadRequestWhenReceivedInvalidPaymentRequest(PaymentRequest paymentRequest,
            out IActionResult actionResult)
        {
            actionResult = null;
            var cardValidator = new PaymentRequestValidator(paymentRequest);
            
            if (paymentRequest.Card == null || paymentRequest.RequestId == Guid.Empty || paymentRequest.Amount == null
                || paymentRequest.MerchantId == Guid.Empty )
            {
                actionResult = ActionResultHelper.ToActionResult(new InvalidCommandResult(paymentRequest.RequestId, "Invalid payment request"));
                return true;
            }

            if (!paymentRequest.Amount.IsValid(out var amountInvalidReason))
            {
                actionResult = ActionResultHelper.ToActionResult(new InvalidCommandResult(paymentRequest.RequestId, amountInvalidReason));
                return true;
            }

            if (!paymentRequest.Card.IsValid(out var cardInvalidReason))
            {
                actionResult = ActionResultHelper.ToActionResult(new InvalidCommandResult(paymentRequest.RequestId, cardInvalidReason));
                return true;
            }

            return false;
        }
    }
}