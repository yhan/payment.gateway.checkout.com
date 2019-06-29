namespace PaymentGateway.Domain
{
    public enum PaymentStatus
    {
        Requested,
        Success,
        RejectedByBank,
        FaultedOnGateway
    }
}