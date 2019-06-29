using System;

namespace PaymentGateway.Domain
{
    public interface IMapAcquiringBankToPaymentGateway
    {
        GatewayPaymentId GetPaymentGatewayId(AcquiringBankPaymentId paymentAcquiringBankId);

        void RememberMapping(AcquiringBankPaymentId acquiringBankPaymentId, GatewayPaymentId gatewayPaymentId);
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