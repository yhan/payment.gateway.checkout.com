using System;
using PaymentGateway.Domain;
using PaymentDetails = PaymentGateway.Domain.PaymentDetails;

namespace PaymentGateway.Infrastructure
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
            return new PaymentDto(payment.RequestId, payment.GatewayPaymentId, payment.AcquiringBankPaymentId, payment.Status);
        }

        public static PaymentDetailsDto AsDto(this PaymentDetails paymentDetails)
        {
            return new PaymentDetailsDto(paymentDetails.BankPaymentId.Value, paymentDetails.CreditCardNumber, paymentDetails.CreditCardHolderName, paymentDetails.CreditCardExpiry, paymentDetails.CreditCardCvv, paymentDetails.Status);
        }
    }
}