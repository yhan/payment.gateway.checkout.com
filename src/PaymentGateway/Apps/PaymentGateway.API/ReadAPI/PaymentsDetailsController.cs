using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.API.WriteAPI;
using PaymentGateway.Domain;

namespace PaymentGateway.API.ReadAPI
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


        public async Task<ActionResult<PaymentDetailsDto>> GetPaymentInfo(Guid paymentAcquiringBankId)
        {
            var paymentGatewayId = _mapper.GetPaymentGatewayId(paymentAcquiringBankId);

            var details = _repository.GetPaymentDetails(paymentGatewayId);

            return details.AsDto();
        }
    }

    public class PaymentDetailsDto
    {
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string CardExpiry { get; set; }
        public string Cvv { get; set; }
        public PaymentGateway.Domain.PaymentStatus Status { get; set; }
    }
}