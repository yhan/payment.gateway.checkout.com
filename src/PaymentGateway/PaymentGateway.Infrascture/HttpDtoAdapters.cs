
using System.Collections.Generic;
using System.Linq;

namespace PaymentGateway.Infrastructure
{
    using System;
    using Domain;

    public static class HttpDtoAdapters
    {
        public static RequestPaymentCommand AsCommand(this PaymentRequest request, Guid gateWayPaymentId)
        {
            var card = new PaymentGateway.Domain.Card(request.Card.Number, request.Card.Expiry, request.Card.Cvv);
            return new RequestPaymentCommand(gateWayPaymentId, request.MerchantId, request.RequestId, card, request.Amount);
        }

        public static PaymentDto AsDto(this Payment payment)
        {
            return new PaymentDto(payment.RequestId, payment.GatewayPaymentId, payment.AcquiringBankPaymentId, payment.Status, payment.Approved);
        }

        public static PaymentDetailsDto AsDto(this PaymentDetails paymentDetails)
        {
            return new PaymentDetailsDto(paymentDetails.BankPaymentId?.Value, new Infrastructure.Card(paymentDetails.Card.Number, paymentDetails.Card.Expiry, paymentDetails.Card.Cvv),  paymentDetails.Status);
        }

        public static IEnumerable<MerchantDto> AsDto(this IEnumerable<Merchant> merchants)
        {
            return merchants.Select(x => new MerchantDto(x.Id, x.Name));
        }
    }
}