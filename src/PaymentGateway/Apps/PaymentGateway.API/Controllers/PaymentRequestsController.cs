using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;

namespace PaymentGateway.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentRequestsController : ControllerBase
    {
        public PaymentRequestsController()
        {
            
        }

        [HttpPost]
        public async Task<IActionResult> ProceedPaymentRequest([FromBody]PaymentRequest paymentRequest, [FromServices]IGenerateGuid guidGenerator)
        {
            var gateWayPaymentId = guidGenerator.Generate();
            var payment = new PaymentDto(paymentRequest.RequestId, gateWayPaymentId, PaymentStatus.Pending);

            return CreatedAtAction(nameof(Get), new {paymentId = 42}, value: payment);

        }

        
        [HttpGet("{paymentId}", Name = nameof(Get))]
        public ActionResult Get([FromRoute]int paymentId)
        {
            return Ok();
        }
    }

    public enum PaymentStatus
    {
        Pending
    }

    public class PaymentDto
    {
        public Guid GateWayPaymentId { get; }
        public PaymentStatus Status { get; }
        public Guid RequestId { get; set; }

        public PaymentDto(Guid requestId, Guid gateWayPaymentId, PaymentStatus status)
        {
            RequestId = requestId;
            GateWayPaymentId = gateWayPaymentId;
            Status = status;
        }
    }
}