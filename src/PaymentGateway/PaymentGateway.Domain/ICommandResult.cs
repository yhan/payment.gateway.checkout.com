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

    public class EntityNotFoundCommandResult : FailureCommandResult
    {
        public EntityNotFoundCommandResult(string message)
            : base(message)
        {
        }

        public EntityNotFoundCommandResult(object id)
            : this($"Entity {id} was not found.")
        {
        }
    }

    public class EntityConflictCommandResult : FailureCommandResult
    {
        public EntityConflictCommandResult(object id)
            : base($"Conflict on entity {id}.")
        {
        }

        //public EntityConflictCommandResult(object id, Timestamp expected, Timestamp actual)
        //    : base($"Conflict on entity {id} expected timestamp {expected}, actual {actual}.")
        //{
        //}
    }

    public class InvalidCommandResult : FailureCommandResult
    {
        public InvalidCommandResult(string reason) : base(reason)
        {

        }
    }
}