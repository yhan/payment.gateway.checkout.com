using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.API.ReadAPI;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using SimpleCQRS;

namespace PaymentGateway.API.WriteAPI
{
    [Route("api/Payments")]
    [ApiController]
    public class PaymentRequestsController : ControllerBase
    {
        private readonly IEventSourcedRepository<Payment> _repository;
        private readonly ILogger<PaymentRequestsController> _logger;
        internal PaymentRequestCommandHandler Handler;
        private readonly ExecutorType _executorType;

        public PaymentRequestsController(IEventSourcedRepository<Payment> repository, IOptionsMonitor<AppSettings> appSettingsAccessor, ILogger<PaymentRequestsController> logger)
        {
            _repository = repository;
            _logger = logger;
            _executorType = appSettingsAccessor.CurrentValue.Executor;
        }

        [HttpPost]
        public async Task<IActionResult> ProceedPaymentRequest([FromBody]PaymentRequest paymentRequest, 
            [FromServices]IGenerateGuid gatewayPaymentIdGenerator,
            [FromServices]IKnowAllPaymentRequests paymentRequests,
            [FromServices]IProcessPayment acquiringBank)
        {
            if (ReturnBadRequestWhenReceivedInvalidPaymentRequest(paymentRequest, out var actionResult))
            {
                return actionResult;
            }

            var gatewayPaymentId = gatewayPaymentIdGenerator.Generate();

            Handler = new PaymentRequestCommandHandler(_repository, paymentRequests, acquiringBank, _executorType == ExecutorType.API ? true : false);
            var commandResult = await Handler.Handle(paymentRequest.AsCommand(gatewayPaymentId));
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