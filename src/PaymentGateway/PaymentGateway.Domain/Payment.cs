using System;
using SimpleCQRS;

namespace PaymentGateway.Domain
{
    public class Payment : AggregateRoot
    {
        public Payment()
        {
        }

        public Payment(Guid gatewayPaymentId, Guid requestId, CreditCard creditCard, Money amount)
        {
            ApplyChange(new PaymentRequested(gatewayPaymentId, requestId, creditCard, amount));
        }


        public override Guid Id => GatewayPaymentId;

        public Guid RequestId { get; private set; }

        public Guid GatewayPaymentId { get; private set; }

        public CreditCard CreditCard { get; private set; }

        public Money Amount { get; private set; }

        public PaymentStatus Status { get; private set; }

        public Guid AcquiringBankId { get; private set; }

        #region decision functions

        public void AcceptPayment(Guid bankPaymentId)
        {
            ApplyChange(new PaymentSucceeded(GatewayPaymentId, bankPaymentId));
        }

        public void RejectPayment(Guid bankPaymentId)
        {
            ApplyChange(new PaymentFailed(GatewayPaymentId, bankPaymentId));
        }

        #endregion

        #region evolution functions

        private void Apply(PaymentRequested evt)
        {
            GatewayPaymentId = evt.GatewayPaymentId;
            RequestId = evt.RequestId;
            Status = PaymentStatus.Pending;
            CreditCard = evt.CreditCard;
            Amount = evt.Amount;

            Version = evt.Version;
        }

        private void Apply(PaymentSucceeded evt)
        {
            AcquiringBankId = evt.BankPaymentId;
            Status = evt.Status;
            Version = evt.Version;
        }

        private void Apply(PaymentFailed evt)
        {
            AcquiringBankId = evt.BankPaymentId;
            Status = evt.Status;
            Version = evt.Version;
        }

        #endregion
    }

}