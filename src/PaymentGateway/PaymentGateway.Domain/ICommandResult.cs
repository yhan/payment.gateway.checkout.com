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
        public Guid PaymentRequestId { get; }

        public InvalidCommandResult(Guid paymentRequestId, string reason) : base(reason)
        {
            PaymentRequestId = paymentRequestId;
        }
    }
}