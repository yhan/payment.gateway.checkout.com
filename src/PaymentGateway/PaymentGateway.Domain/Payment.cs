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


        private void Apply(PaymentRequested evt)
        {
            GatewayPaymentId = evt.GatewayPaymentId;
            RequestId = evt.RequestId;

            Status = PaymentStatus.Pending;

            //this.CreditCard = evt.CreditCard;
        }


        public override Guid Id  => GatewayPaymentId;

        public Guid RequestId { get; private set; }

        public Guid GatewayPaymentId { get; private set; }

        public PaymentStatus Status { get; private set; }


    }
}