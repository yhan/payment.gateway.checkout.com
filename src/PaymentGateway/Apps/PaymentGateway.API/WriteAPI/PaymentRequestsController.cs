using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PaymentGateway.API.ReadAPI;
using PaymentGateway.Domain;
using SimpleCQRS;

[assembly: InternalsVisibleTo("PaymentGateway.Tests")]

namespace PaymentGateway.API.WriteAPI
{
    [Route("api/PaymentRequests")]
    [ApiController]
    public class PaymentRequestsController : ControllerBase
    {
        private readonly IEventSourcedRepository<Payment> _repository;
        internal PaymentRequestCommandHandler Handler;
        private ExecutorType _executorType;

        public PaymentRequestsController(IEventSourcedRepository<Payment> repository, IOptionsMonitor<AppSettings> appSettingsAccessor)
        {
            _repository = repository;
            _executorType = appSettingsAccessor.CurrentValue.Executor;
        }

        [HttpPost]
        public async Task<IActionResult> ProceedPaymentRequest([FromBody]PaymentRequest paymentRequest, 
            [FromServices]IGenerateGuid guidGenerator,
            [FromServices]IProvidePaymentIdsMapping paymentIdsMapping,
            [FromServices]IProcessPayment acquiringBank)
        {
            var gatewayPaymentId = guidGenerator.Generate();

            Handler = new PaymentRequestCommandHandler(_repository, paymentIdsMapping, acquiringBank, _executorType == ExecutorType.API ? true : false);
            var commandResult = await Handler.Handle(paymentRequest.AsCommand(gatewayPaymentId));
            switch (commandResult)
            {
                case SuccessCommandResult<Payment> success:
                    var paymentDto = success.Entity.AsDto();

                    return CreatedAtRoute( nameof(PaymentReadController.GetPaymentInfo), 
                        routeValues: new {gateWayPaymentId = paymentDto.GateWayPaymentId}, 
                        value: paymentDto);

                case InvalidCommandResult invalid:
                    return ActionResultHelper.ToActionResult(invalid);

                default:
                    throw new NotSupportedException();
            }
        }
        
    }
}