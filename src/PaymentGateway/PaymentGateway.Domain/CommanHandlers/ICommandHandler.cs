using System.Threading.Tasks;
using PaymentGateway.Domain.Commands;

namespace PaymentGateway.Domain
{
    public interface ICommandHandler<in TCommand> where TCommand : Command
    {
        Task<ICommandResult> Handle(TCommand command);
    }
}