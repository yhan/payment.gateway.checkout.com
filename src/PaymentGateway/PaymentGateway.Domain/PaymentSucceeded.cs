using System;
using SimpleCQRS;

namespace PaymentGateway.Domain
{
    public class PaymentSucceeded : Event
    {
        public PaymentStatus Status = PaymentStatus.Success;

        public PaymentSucceeded(Guid gatewayPaymentId, Guid bankPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
            BankPaymentId = bankPaymentId;
        }

        public Guid GatewayPaymentId { get; }
        public Guid BankPaymentId { get; }
    }

    public class PaymentFailed : Event
    {
        public PaymentStatus Status = PaymentStatus.Failure;

        public PaymentFailed(Guid gatewayPaymentId, Guid bankPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
            BankPaymentId = bankPaymentId;
        }

        public Guid GatewayPaymentId { get; }
        public Guid BankPaymentId { get; }
    }

}