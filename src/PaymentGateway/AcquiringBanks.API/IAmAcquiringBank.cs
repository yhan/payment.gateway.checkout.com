using System.Threading.Tasks;

namespace AcquiringBanks.API
{
    public interface IAmAcquiringBank
    {
        Task<string> RespondsTo(string serializeObject);
    }
}