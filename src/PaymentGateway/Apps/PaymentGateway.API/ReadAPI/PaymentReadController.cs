using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;
using SimpleCQRS;

namespace PaymentGateway.API.ReadAPI
{
    [Route("api/Payments")]
    [ApiController]
    public class PaymentReadController
    {
        private readonly IEventSourcedRepository<Payment> _repository;

        public PaymentReadController(IEventSourcedRepository<Payment> repository)
        {
            _repository = repository;
        }

        [HttpGet("{gateWayPaymentId}", Name = nameof(GetPaymentInfo))]
        public async Task<ActionResult<Payment>> GetPaymentInfo([FromRoute]Guid gateWayPaymentId)
        {
            //var etag = Request.GetTypedHeaders().IfNoneMatch?.FirstOrDefault();
            var payment = await _repository.GetById(gateWayPaymentId);

            return payment;
        }

    }
}