using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;
using PaymentGateway.Infrastructure;
using PaymentGateway.ReadAPI;

namespace PaymentGateway.Tests
{
    [TestFixture]
    public class ReadMerchantsShould
    {
        [Test]
        public async Task Can_read_all_onboarded_merchants()
        {
            var controller = new MerchantsController(new MerchantsRepository());
            IEnumerable<MerchantDto> merchants = (await controller.GetAllMerchants()).ToArray();

            Check.That(merchants).HasSize(2);
            Check.That(merchants).IsOnlyMadeOf(
                new MerchantDto(MerchantsRepository.Amazon, nameof(MerchantsRepository.Amazon)),
                new MerchantDto(MerchantsRepository.Apple, nameof(MerchantsRepository.Apple)));
        }
    }
}