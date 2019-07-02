using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain;

namespace PaymentGateway.API.ReadAPI
{
    /// <summary>
    /// Not public API, only for facilitate testing
    /// </summary>
    [Route("api/GatewayPaymentsIds")]
    [ApiController]
    public class GatewayPaymentsIdsController : ControllerBase
    {
        private readonly IKnowAllPaymentsIds _paymentsIdsRepository;

        public GatewayPaymentsIdsController(IKnowAllPaymentsIds paymentsIdsRepository)
        {
            _paymentsIdsRepository = paymentsIdsRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Guid>> Get()
        {
            var gatewayPaymentIds = await _paymentsIdsRepository.AllGatewayPaymentsIds();
            return gatewayPaymentIds.Select(x => x.Value);
        }
    }
}