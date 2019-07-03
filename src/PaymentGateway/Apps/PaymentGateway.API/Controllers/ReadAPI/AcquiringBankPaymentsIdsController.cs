using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.ReadAPI
{
    /// <summary>
    /// Not public API, only for facilitate testing.
    /// Get all acquiring bank payments ids
    /// </summary>
    [Route("api/AcquiringBankPaymentsIds")]
    [ApiController]
    public class AcquiringBankPaymentsIdsController : ControllerBase
    {
        private readonly IKnowAllPaymentsIds _paymentsIdsRepository;

        public AcquiringBankPaymentsIdsController(IKnowAllPaymentsIds paymentsIdsRepository)
        {
            _paymentsIdsRepository = paymentsIdsRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Guid>> Get()
        {
            var gatewayPaymentIds = await _paymentsIdsRepository.AllAcquiringBankPaymentsIds();
            return gatewayPaymentIds.Select(x => x.Value);
        }
    }

    [Route("api/Merchants")]
    [ApiController]
    public class MerchantsController : ControllerBase
    {
        private readonly IKnowAllMerchants _merchantsRepository;

        public MerchantsController(IKnowAllMerchants merchantsRepository)
        {
            _merchantsRepository = merchantsRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<MerchantDto>> GetAllMerchants()
        {
            return (await _merchantsRepository.GetAllMerchants()).AsDto();
        }
    }
}