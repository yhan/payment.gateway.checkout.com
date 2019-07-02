using System;
using PaymentGateway.Domain.Events;

namespace PaymentGateway.Domain
{
    public class Payment : AggregateRoot
    {
        public Payment()
        {
        }

        public Payment(Guid gatewayPaymentId, Guid merchantId, Guid requestId, Card card, Money amount)
        {
            ApplyChange(new PaymentRequested(gatewayPaymentId, merchantId, requestId, card, amount));
        }


        public override Guid Id => GatewayPaymentId;

        public Guid RequestId { get; private set; }

        public Guid GatewayPaymentId { get; private set; }

        public Card Card { get; private set; }

        public Money Amount { get; private set; }

        public PaymentStatus Status { get; private set; }

        public Guid? AcquiringBankPaymentId { get; private set; }
        
        public Guid MerchantId { get; private set; }

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

        public void BankConnectionFails()
        {
            ApplyChange(new PaymentFailedBecauseBankUnavailable(GatewayPaymentId));
        }

        #endregion

        #region evolution functions, dynamically invoked

        private void Apply(PaymentRequested evt)
        {
            GatewayPaymentId = evt.GatewayPaymentId;
            MerchantId = evt.MerchantId;
            RequestId = evt.RequestId;
            Status = PaymentStatus.Pending;
            Card = evt.Card;
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

        private void Apply(PaymentFailedBecauseBankUnavailable evt)
        {
            Status = evt.Status;
            Version = evt.Version;
        }

        #endregion
    }

    public class PaymentFailedBecauseBankUnavailable : Event
    {
        public Guid GatewayPaymentId { get; }

        public PaymentStatus Status = PaymentStatus.BankUnavailable;

        public PaymentFailedBecauseBankUnavailable(Guid gatewayPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
        }
    }
}