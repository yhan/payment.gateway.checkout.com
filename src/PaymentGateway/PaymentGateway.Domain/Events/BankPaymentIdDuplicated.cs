using System;

namespace PaymentGateway.Domain.Events
{
    public class BankPaymentIdDuplicated : AggregateEvent
    {
        public BankPaymentIdDuplicated(Guid gatewayPaymentId): base(gatewayPaymentId)
        {
        }

        public PaymentStatus Status = PaymentStatus.ReceivedDuplicatedBankPaymentIdFailure;
    }
}