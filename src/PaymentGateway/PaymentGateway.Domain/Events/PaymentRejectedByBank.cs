using System;
using SimpleCQRS;

namespace PaymentGateway.Domain.Events
{
    public class PaymentRejectedByBank : Event
    {
        public PaymentStatus Status = PaymentStatus.RejectedByBank;

        public PaymentRejectedByBank(Guid gatewayPaymentId, Guid bankPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
            BankPaymentId = bankPaymentId;
        }

        public Guid GatewayPaymentId { get; }
        public Guid BankPaymentId { get; }
    }
}