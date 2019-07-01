using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.API.ReadAPI;
using PaymentGateway.Domain;
using SimpleCQRS;

[assembly: InternalsVisibleTo("PaymentGateway.Tests")]

namespace PaymentGateway.API.WriteAPI
{
    public class PaymentRequestValidator
    {
        private readonly PaymentRequest _paymentRequest;

        public PaymentRequestValidator(PaymentRequest paymentRequest)
        {
            _paymentRequest = paymentRequest;
        }

        public bool CardCvvInvalid()
        {
            var reg = "^[0-9]{3}$";
            return !Regex.IsMatch(_paymentRequest.Cvv, reg);
        }

        public  bool CardNumberInvalid()
        {
            var reg = "^[0-9]{4} [0-9]{4} [0-9]{4} [0-9]{4}$";
            return !Regex.IsMatch(_paymentRequest.CardNumber, reg);
        }
    }

    [Route("api/PaymentRequests")]
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
            [FromServices]IProvidePaymentIdsMapping paymentIdsMapping,
            [FromServices]IProcessPayment acquiringBank)
        {
            var creditCardValidator = new PaymentRequestValidator(paymentRequest);

            if (creditCardValidator.CardNumberInvalid())
            {
                return ActionResultHelper.ToActionResult(new InvalidCommandResult("Invalid credit card number"));
            }

            if (creditCardValidator.CardCvvInvalid())
            {
                return ActionResultHelper.ToActionResult(new InvalidCommandResult("Invalid credit card CVV"));
            }

            var gatewayPaymentId = gatewayPaymentIdGenerator.Generate();

            Handler = new PaymentRequestCommandHandler(_repository, paymentIdsMapping, acquiringBank, _executorType == ExecutorType.API ? true : false);
            var commandResult = await Handler.Handle(paymentRequest.AsCommand(gatewayPaymentId));
            switch (commandResult)
            {
                case SuccessCommandResult<Payment> success:
                    var paymentDto = success.Entity.AsDto();

                    return CreatedAtRoute( nameof(PaymentReadController.GetPaymentInfo), 
                        routeValues: new {gateWayPaymentId = paymentDto.GatewayPaymentId}, 
                        value: paymentDto);

                case InvalidCommandResult invalid:
                    return ActionResultHelper.ToActionResult(invalid);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}