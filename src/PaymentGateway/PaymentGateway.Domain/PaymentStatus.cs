namespace PaymentGateway.Domain
{
    public enum PaymentStatus
    {
        Pending,
        Success,
        RejectedByBank,
        FaultedOnGateway,
        BankUnavailable
    }
}