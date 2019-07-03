using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    /// <summary>
    /// Glue component (mediator) which calls bank facade <see cref="IAdaptToBank"/>.
    /// Then do necessary changes in PaymentGateway's domain.
    /// </summary>
    public interface IProcessPayment
    {
        Task AttemptPaying(IAdaptToBank bankAdapter, PayingAttempt payingAttempt);
    }
}