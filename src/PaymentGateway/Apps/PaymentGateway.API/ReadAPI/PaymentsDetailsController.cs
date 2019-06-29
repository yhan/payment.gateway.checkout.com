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

        [HttpGet("{paymentAcquiringBankId}")]
        public async Task<ActionResult<PaymentDetailsDto>> GetPaymentInfo(Guid paymentAcquiringBankId)
        {
            var paymentGatewayId = _mapper.GetPaymentGatewayId(paymentAcquiringBankId);

            var details = await _repository.GetPaymentDetails(paymentGatewayId);

            return details.AsDto();
        }
    }

    public class PaymentDetailsDto
    {
        public string CreditCardNumber { get; }
        public string CreditCardHolderName { get; }
        public string CreditCardExpiry { get; }
        public string CreditCardCvv { get; }
        public PaymentGateway.Domain.PaymentStatus Status { get; }


        public PaymentDetailsDto(string creditCardNumber, string creditCardHolderName, string creditCardExpiry, string creditCardCvv, PaymentStatus status)
        {
            CreditCardNumber = creditCardNumber;
            CreditCardHolderName = creditCardHolderName;
            CreditCardExpiry = creditCardExpiry;
            CreditCardCvv = creditCardCvv;
            Status = status;
        }
    }
}