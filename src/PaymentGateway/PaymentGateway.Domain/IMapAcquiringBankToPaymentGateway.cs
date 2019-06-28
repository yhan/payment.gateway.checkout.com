using System;

namespace PaymentGateway.Domain
{
    public interface IMapAcquiringBankToPaymentGateway
    {
        Guid GetPaymentGatewayId(Guid gatewayPaymentId);
        void RememberMapping(PaymentIds paymentIds);
    }


    public struct PaymentIds
    {
        public Guid BankPaymentId { get; }
        public Guid GatewayPaymentId { get; }

        public PaymentIds(Guid bankPaymentId, Guid gatewayPaymentId)
        {
            BankPaymentId = bankPaymentId;
            GatewayPaymentId = gatewayPaymentId;
        }
    }
}