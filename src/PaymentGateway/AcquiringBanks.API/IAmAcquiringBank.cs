using System.Threading.Tasks;

namespace AcquiringBanks.API
{

    /// <summary>
    /// Acquiring bank abstraction which allows `Gateway` to different merchants' banks
    /// an request payments
    /// </summary>
    public interface IAmAcquiringBank
    {
        Task<string> RespondsTo(string serializeObject);
        Task<bool> Connect();
    }
}