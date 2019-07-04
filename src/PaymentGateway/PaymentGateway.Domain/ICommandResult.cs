using System;

namespace PaymentGateway.Domain
{
    public interface ICommandResult
    {
    }

    public class FailureCommandResult : ICommandResult
    {
        public string Reason { get; }

        public FailureCommandResult(string reason)
        {
            Reason = reason;
        }
    }

    public class InvalidCommandResult : FailureCommandResult
    {
        public InvalidCommandResult(string reason) : base(reason)
        {
        }
    }
}