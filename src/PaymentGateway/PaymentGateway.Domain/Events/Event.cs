namespace PaymentGateway.Domain.Events
{
    public abstract class Event : IMessage
    {
        public int Version;
    }
}