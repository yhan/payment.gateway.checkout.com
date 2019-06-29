using System;
using PaymentGateway.Domain.Events;
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

        public Guid AcquiringBankPaymentId { get; private set; }

        #region decision functions

        public void AcceptPayment(Guid bankPaymentId)
        {
            ApplyChange(new PaymentSucceeded(GatewayPaymentId, bankPaymentId));
        }

        public void BankRejectPayment(Guid bankPaymentId)
        {
            ApplyChange(new PaymentRejectedByBank(GatewayPaymentId, bankPaymentId));
        }

        public void FailOnGateway(Guid bankPaymentId)
        {
            ApplyChange(new PaymentFaulted(GatewayPaymentId, bankPaymentId));

        }

        #endregion

        #region evolution functions

        private void Apply(PaymentRequested evt)
        {
            GatewayPaymentId = evt.GatewayPaymentId;
            RequestId = evt.RequestId;
            Status = PaymentStatus.Requested;
            CreditCard = evt.CreditCard;
            Amount = evt.Amount;

            Version = evt.Version;
        }

        private void Apply(PaymentSucceeded evt)
        {
            AcquiringBankPaymentId = evt.BankPaymentId;
            Status = evt.Status;
            Version = evt.Version;
        }

        private void Apply(PaymentRejectedByBank evt)
        {
            AcquiringBankPaymentId = evt.BankPaymentId;
            Status = evt.Status;
            Version = evt.Version;
        }

        private void Apply(PaymentFaulted evt)
        {
            Status = evt.Status;
            Version = evt.Version;
        }

        #endregion
    }
}