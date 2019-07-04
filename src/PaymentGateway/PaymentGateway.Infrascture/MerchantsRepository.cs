using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class MerchantsRepository : IKnowAllMerchants
    {
        public async Task<IEnumerable<Merchant>> GetAllMerchants()
        {
            return await Task.FromResult(new[]
            {
                new Merchant(Amazon, nameof(Amazon)),
                new Merchant(Apple, nameof(Apple)),
                new Merchant(FailFromThe2ndPaymentMerchant, nameof(FailFromThe2ndPaymentMerchant)),
            });
        }

        public static Guid Amazon = Guid.Parse("2d0ae468-7ac9-48f4-be3f-73628de3600e");
        public static Guid Apple= Guid.Parse("06c6116f-1d4e-44d3-ae9f-8df90f991a52");
        public static Guid FailFromThe2ndPaymentMerchant = Guid.Parse("8d443f3b-55a3-4931-ba4a-3fa771bb1066");
    }
}