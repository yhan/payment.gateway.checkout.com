using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    public interface IKnowAllMerchants
    {
        Task<IEnumerable<Merchant>> GetAllMerchants();
    }
}