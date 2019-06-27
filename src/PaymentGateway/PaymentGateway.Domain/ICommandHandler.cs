using System.Threading.Tasks;
using SimpleCQRS;

namespace PaymentGateway.Domain
{
    public interface ICommandHandler<in TCommand> where TCommand : Command
    {
        Task<ICommandResult> Handle(TCommand command);
    }
}