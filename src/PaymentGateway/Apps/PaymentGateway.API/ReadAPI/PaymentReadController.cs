using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using SimpleCQRS;

namespace PaymentGateway.API.ReadAPI
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
            //var etag = Request.GetTypedHeaders().IfNoneMatch?.FirstOrDefault();
            try
            {
                var payment = await _repository.GetById(gateWayPaymentId);
                return payment.AsDto();
            }
            catch (AggregateNotFoundException)
            {
                return NotFound();
            }
        }
    }
}