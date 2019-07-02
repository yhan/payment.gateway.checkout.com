using System;
using PaymentGateway.Domain;
using PaymentDetails = PaymentGateway.Domain.PaymentDetails;

namespace PaymentGateway.Infrastructure
{
    public static class HttpDtoAdapters
    {
        public static RequestPaymentCommand AsCommand(this PaymentRequest request, Guid gateWayPaymentId)
        {
            var card = new Card(request.CardNumber, request.Cvv, request.Expiry);
            return new RequestPaymentCommand(gateWayPaymentId, request.MerchantId, request.RequestId, card, request.Amount);
        }

        public static PaymentDto AsDto(this Payment payment)
        {
            return new PaymentDto(payment.RequestId, payment.GatewayPaymentId, payment.AcquiringBankPaymentId, payment.Status);
        }

        public static PaymentDetailsDto AsDto(this PaymentDetails paymentDetails)
        {
            return new PaymentDetailsDto(paymentDetails.BankPaymentId.Value, paymentDetails.CardNumber, paymentDetails.CardExpiry, paymentDetails.CardCvv, paymentDetails.Status);
        }
    }
}