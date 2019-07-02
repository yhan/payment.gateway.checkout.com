using System;

namespace AcquiringBanks.Stub
{
    /// <summary>
    /// The response returned by the acquiring bank `Societe Generale`.
    /// Is part of the Societe Generale payment API.
    /// </summary>
    public class SocieteGeneraleResponse
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

    /// <summary>
    /// The response returned by the acquiring bank `BNP`.
    /// Is part of the Societe Generale payment API.
    /// </summary>
    public class BNPResponse
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