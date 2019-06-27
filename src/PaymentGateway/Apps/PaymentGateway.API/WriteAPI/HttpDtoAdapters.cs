using System;
using PaymentGateway.Domain;

namespace PaymentGateway.API.WriteAPI
{
    public static class HttpDtoAdapters
    {
        public static RequestPaymentCommand AsCommand(this PaymentRequest request, Guid gateWayPaymentId)
        {
            var creditCard = new CreditCard(request.CardNumber, request.Cvv, request.Expiry, request.CardHolderName);
            return new RequestPaymentCommand(gateWayPaymentId, request.RequestId, creditCard, request.Amount);
        }

        public static PaymentDto AsDto(this Payment payment)
        {
            return new PaymentDto(payment.RequestId, payment.GatewayPaymentId, payment.Status);
        }
    }
}