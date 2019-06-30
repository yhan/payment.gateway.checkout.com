using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentGateway.Domain
{
    public interface IMapAcquiringBankToPaymentGateway : IKnowAllPaymentsIds
    {
        GatewayPaymentId GetPaymentGatewayId(AcquiringBankPaymentId paymentAcquiringBankId);

        void RememberMapping(AcquiringBankPaymentId acquiringBankPaymentId, GatewayPaymentId gatewayPaymentId);
    }

    public interface IKnowAllPaymentsIds
    {
        Task<ICollection<GatewayPaymentId>> AllGatewayPaymentsIds();

        Task<ICollection<AcquiringBankPaymentId>> AllAcquiringBankPaymentsIds();
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