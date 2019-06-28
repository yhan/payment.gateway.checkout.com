using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.API.ReadAPI;
using PaymentGateway.Domain;
using SimpleCQRS;

namespace PaymentGateway.API.WriteAPI
{
    [Route("api/PaymentRequests")]
    [ApiController]
    public class PaymentRequestsController : ControllerBase
    {
        private readonly IEventSourcedRepository<Payment> _repository;

        public PaymentRequestsController(IEventSourcedRepository<Payment> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> ProceedPaymentRequest([FromBody]PaymentRequest paymentRequest, 
            [FromServices]IGenerateGuid guidGenerator,
            [FromServices]IProvidePaymentIdsMapping paymentIdsMapping,
            [FromServices]IProcessPayment acquiringBank)
        {
            var gatewayPaymentId = guidGenerator.Generate();

            var handler = new PaymentRequestCommandHandler(_repository, paymentIdsMapping, acquiringBank);
            var commandResult = await handler.Handle(paymentRequest.AsCommand(gatewayPaymentId));
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