using System;
using AcquiringBanks.API;

namespace AcquiringBanks.Stub
{
    public class SocieteGeneraleResponse : IBankResponse
    {
        public Guid BankPaymentId { get; }
        public Guid GatewayPaymentId { get; }
        public BankPaymentStatus PaymentStatus { get; }

        public SocieteGeneraleResponse(Guid bankPaymentId, Guid gatewayPaymentId, BankPaymentStatus paymentStatus)
        {
            BankPaymentId = bankPaymentId;
            GatewayPaymentId = gatewayPaymentId;
            PaymentStatus = paymentStatus;
        }
    }

    public class BNPResponse: IBankResponse
    {
        public Guid BankPaymentId { get; }
        public Guid GatewayPaymentId { get; }
        public BankPaymentStatus PaymentStatus { get; }

        public BNPResponse(Guid bankPaymentId, Guid gatewayPaymentId, BankPaymentStatus paymentStatus)
        {
            BankPaymentId = bankPaymentId;
            GatewayPaymentId = gatewayPaymentId;
            PaymentStatus = paymentStatus;
        }
    }
}