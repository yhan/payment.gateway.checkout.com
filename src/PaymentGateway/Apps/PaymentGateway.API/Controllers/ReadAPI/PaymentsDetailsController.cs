using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.ReadAPI
{
    [Route("api/PaymentsDetails")]
    [ApiController]
    public class PaymentsDetailsController : ControllerBase
    {
        private readonly IMapAcquiringBankToPaymentGateway _mapper;
        private readonly IPaymentDetailsRepository _repository;

        public PaymentsDetailsController(IMapAcquiringBankToPaymentGateway mapper, IPaymentDetailsRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        [HttpGet("{acquiringBankPaymentId}")]
        public async Task<ActionResult<PaymentDetailsDto>> GetPaymentDetails(Guid acquiringBankPaymentId)
        {
            try
            {
                var paymentGatewayId = _mapper.GetPaymentGatewayId(new AcquiringBankPaymentId(acquiringBankPaymentId));

                var details = await _repository.GetPaymentDetails(paymentGatewayId);

                return details.AsDto();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(acquiringBankPaymentId);
            }
        }
    }
}