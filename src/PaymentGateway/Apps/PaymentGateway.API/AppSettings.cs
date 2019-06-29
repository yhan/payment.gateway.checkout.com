namespace PaymentGateway.API
{
    public class AppSettings
    {
        public ExecutorType Executor { get; set; }
    }

    public enum ExecutorType
    {
        API,
        Tests
    }

}