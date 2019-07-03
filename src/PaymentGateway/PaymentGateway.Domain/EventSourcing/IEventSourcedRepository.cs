using System;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    public interface IEventSourcedRepository<T> where T : AggregateRoot
    {
        /// <summary>
        ///     Save <paramref name="aggregate" /> uncommitted changes to underling storage
        /// </summary>
        /// <param name="aggregate">The aggregate or entity</param>
        /// <param name="expectedVersion">
        ///     The system use optimistic lock, expected version is the version of aggregate when the
        ///     system read before doing changes and saving
        /// </param>
        /// <returns>Task representing the eventual success of saving, or faulting, or timeout</returns>
        Task Save(AggregateRoot aggregate, int expectedVersion);

        /// <summary>
        ///     Get the aggregate or entity using its id
        /// </summary>
        /// <param name="id">The id of aggregate or entity</param>
        /// <returns>The aggregate or entity</returns>
        Task<T> GetById(Guid id);
    }
}