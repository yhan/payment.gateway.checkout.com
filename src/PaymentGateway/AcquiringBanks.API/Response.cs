using System;
using AcquiringBanks.API;

public class Response
{
    public Guid BankPaymentId { get; }
    public Guid GatewayPaymentId { get; }
    public BankPaymentStatus PaymentStatus { get; }

    public Response(Guid bankPaymentId, Guid gatewayPaymentId, BankPaymentStatus paymentStatus)
    {
        BankPaymentId = bankPaymentId;
        GatewayPaymentId = gatewayPaymentId;
        PaymentStatus = paymentStatus;
    }
}