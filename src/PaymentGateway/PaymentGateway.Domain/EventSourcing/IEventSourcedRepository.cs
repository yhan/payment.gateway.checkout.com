using System;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    public interface IEventSourcedRepository<T> where T : AggregateRoot
    {
        Task Save(AggregateRoot aggregate, int expectedVersion);

        Task<T> GetById(Guid id);
    }
}