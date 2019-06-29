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

        [HttpGet("{acquiringBankPaymentId}")]
        public async Task<ActionResult<PaymentDetailsDto>> GetPaymentInfo(Guid acquiringBankPaymentId)
        {
            var paymentGatewayId = _mapper.GetPaymentGatewayId(new AcquiringBankPaymentId(acquiringBankPaymentId));

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
        public Guid AcquiringBankPaymentId { get; set; }


        public PaymentDetailsDto(AcquiringBankPaymentId acquiringBankPaymentId, string creditCardNumber,
            string creditCardHolderName, string creditCardExpiry, string creditCardCvv, PaymentStatus status)
        {
            AcquiringBankPaymentId = acquiringBankPaymentId.Value;

            CreditCardNumber = creditCardNumber;
            CreditCardHolderName = creditCardHolderName;
            CreditCardExpiry = creditCardExpiry;
            CreditCardCvv = creditCardCvv;
            Status = status;
        }
    }
}