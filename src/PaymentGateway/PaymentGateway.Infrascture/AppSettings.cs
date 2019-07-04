namespace PaymentGateway.Infrastructure
{
    public class AppSettings
    {
        public ExecutorType Executor { get; set; }

        public int TimeoutInMilliseconds { get; set; }
        public int MaxBankLatencyInMilliseconds { get; set; }
    }
}