namespace PaymentGateway.Domain
{
    public enum PaymentStatus
    {
        /// <summary>
        ///     Payment requested by `Merchant`, waiting for `Acquiring bank`'s confirmation
        /// </summary>
        Pending,

        /// <summary>
        ///     Payment succeeded
        /// </summary>
        Success,

        /// <summary>
        ///     Payment rejected by the  `Acquiring bank`
        /// </summary>
        RejectedByBank,

        /// <summary>
        ///     Payment processing faulted on `Gateway`
        /// </summary>
        FaultedOnGateway,

        /// <summary>
        ///     Can't reach `Acquiring bank`'s API
        /// </summary>
        BankUnavailable,

        /// <summary>
        ///     Payment processing timeout, may caused by gateway internal or a bank timeout
        /// </summary>
        Timeout,
        ReceivedDuplicatedBankPaymentIdFailure
    }
}