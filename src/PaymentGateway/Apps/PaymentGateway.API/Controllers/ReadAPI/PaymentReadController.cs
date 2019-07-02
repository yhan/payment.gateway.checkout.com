using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.ReadAPI
{
    [Route("api/Payments")]
    [ApiController]
    public class PaymentReadController : ControllerBase
    {
        private readonly IEventSourcedRepository<Payment> _repository;

        public PaymentReadController(IEventSourcedRepository<Payment> repository)
        {
            _repository = repository;
        }

        [HttpGet("{gateWayPaymentId}", Name = nameof(GetPaymentInfo))]
        public async Task<ActionResult<PaymentDto>> GetPaymentInfo([FromRoute]Guid gateWayPaymentId)
        {
            // Etag can be added when the `PaymentDto` becomes to large
            // When `PaymentDto` not changed, send NotChanged back to client.
            //var etag = Request.GetTypedHeaders().IfNoneMatch?.FirstOrDefault();
            try
            {
                var payment = await _repository.GetById(gateWayPaymentId);
                return payment.AsDto();
            }
            catch (AggregateNotFoundException)
            {
                return NotFound(gateWayPaymentId);
            }
        }
    }
}