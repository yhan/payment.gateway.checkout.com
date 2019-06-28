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
            GatewayPaymentId = gatewayPaymentId;
            ApplyChange(new PaymentRequested(gatewayPaymentId, requestId, creditCard, amount));
        }


        public override Guid Id => GatewayPaymentId;

        public Guid RequestId { get; private set; }

        public Guid GatewayPaymentId { get; private set; }

        public PaymentStatus Status { get; private set; }

        public Guid AcquiringBankId { get; private set; }

        #region decision functions

        public void AcceptPayment(BankResponse bankResponse)
        {
            ApplyChange(new PaymentSucceeded(GatewayPaymentId, bankResponse.BankPaymentId));
        }

        #endregion

        #region evolution functions

        private void Apply(PaymentRequested evt)
        {
            GatewayPaymentId = evt.GatewayPaymentId;
            RequestId = evt.RequestId;
            Status = PaymentStatus.Pending;
            Version = evt.Version;

            //this.CreditCard = evt.CreditCard;
        }

        private void Apply(PaymentSucceeded evt)
        {
            AcquiringBankId = evt.BankPaymentId;
            Status = evt.Status;
            Version = evt.Version;
        }

        #endregion
    }
}