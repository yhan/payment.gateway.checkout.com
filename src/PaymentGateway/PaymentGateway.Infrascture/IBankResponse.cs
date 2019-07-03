using System;
using AcquiringBanks.Stub;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class BankDoesNotRespond : IBankResponse
    {
        public Guid GatewayPaymentId { get; }

        public BankDoesNotRespond(Guid gatewayPaymentId)
        {
            GatewayPaymentId = gatewayPaymentId;
        }
    }

    public class BankResponse : IBankResponse
    {
        public Guid BankPaymentId { get; }
        public Guid GatewayPaymentId { get; }
        public BankPaymentStatus PaymentStatus { get; }

        public BankResponse(Guid bankPaymentId, Guid gatewayPaymentId, BankPaymentStatus paymentStatus)
        {
            BankPaymentId = bankPaymentId;
            GatewayPaymentId = gatewayPaymentId;
            PaymentStatus = paymentStatus;
        }
    }
}