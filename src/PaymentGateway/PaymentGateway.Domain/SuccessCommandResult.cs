namespace PaymentGateway.Domain
{
    public class SuccessCommandResult<TResult> : SuccessCommandResult
    {
        public SuccessCommandResult(TResult entity)
        {
            Entity = entity;
        }

        public TResult Entity { get; }
    }

    public class SuccessCommandResult : ICommandResult
    {
        public static SuccessCommandResult<TResult> WithResult<TResult>(TResult result)
        {
            return new SuccessCommandResult<TResult>(result);
        }
    }
}