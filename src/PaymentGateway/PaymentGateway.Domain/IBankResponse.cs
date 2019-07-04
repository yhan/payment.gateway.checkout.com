using System;

namespace PaymentGateway.Domain
{
    public interface IBankResponse
    {
        Guid GatewayPaymentId { get; }
    }

    public class NullBankResponse : IBankResponse
    {
        public Guid GatewayPaymentId { get; } = Guid.Empty;
    }
}