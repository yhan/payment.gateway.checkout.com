namespace PaymentGateway.Infrastructure
{
    /// <summary>
    ///     Hosting process type.
    /// </summary>
    public enum ExecutorType
    {
        /// <summary>
        ///     The process is running in API
        /// </summary>
        API,

        /// <summary>
        ///     The process is running in a testing sessions (NUnit)
        /// </summary>
        Tests
    }
}