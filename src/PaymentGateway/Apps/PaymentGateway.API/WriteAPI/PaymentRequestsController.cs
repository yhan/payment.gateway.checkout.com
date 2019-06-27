using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> ProceedPaymentRequest([FromBody]PaymentRequest paymentRequest, [FromServices]IGenerateGuid guidGenerator)
        {
            var gatewayPaymentId = guidGenerator.Generate();

            var handler = new PaymentRequestCommandHandler(_repository);
            await handler.Handle(paymentRequest.AsCommand(gatewayPaymentId));

            // Make sure that relevant events are stored to event store
            var payment = await _repository.GetById(gatewayPaymentId);
            

            return CreatedAtAction(actionName: "GetPaymentInfo", routeValues: new {gateWayPaymentId = Guid.NewGuid()}, value: payment.AsDto());
        }

        [HttpGet("{gateWayPaymentId}", Name = nameof(GetPaymentInfo))]
        public ActionResult GetPaymentInfo([FromRoute]Guid gateWayPaymentId)
        {
            return Ok();
        }
        
    }
}