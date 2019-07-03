namespace PaymentGateway.Domain
{
    public interface IKnowSendRequestToBankSynchrony
    {
        bool SendPaymentRequestAsynchronously();
    }
}