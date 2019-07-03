using System;
using PaymentGateway.Domain.Commands;

namespace PaymentGateway.Domain
{
    public static class CommandHandlerExtensions
    {
        public static ICommandResult Success<TCommand, TEntity>(this ICommandHandler<TCommand> handler, TEntity entity)
            where TCommand : Command
        {
            return new SuccessCommandResult<TEntity>(entity);
        }

        public static ICommandResult Invalid<TCommand>(this ICommandHandler<TCommand> handler, Guid commandId, string reason)
            where TCommand : Command
        {
            return new InvalidCommandResult(commandId, reason);
        }

        public static ICommandResult Failure<TCommand>(this ICommandHandler<TCommand> handler, string reason)
            where TCommand : Command
        {
            return new FailureCommandResult(reason);
        }
    }
}