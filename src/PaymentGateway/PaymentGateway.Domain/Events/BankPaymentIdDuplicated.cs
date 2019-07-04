using System;

namespace PaymentGateway.Domain.Events
{
    /// <summary>
    ///     Raised when bank Bank sends payment id which conflicts with a previously received one.
    /// </summary>
    public class BankPaymentIdDuplicated : AggregateEvent
    {
        public BankPaymentIdDuplicated(Guid gatewayPaymentId): base(gatewayPaymentId)
        {
        }

        public PaymentStatus Status = PaymentStatus.ReceivedDuplicatedBankPaymentIdFailure;
    }
}