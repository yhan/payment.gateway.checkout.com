﻿using System;
using SimpleCQRS;

namespace PaymentGateway.Domain
{
    public class PaymentRequested : Event
    {
        public Guid GatewayPaymentId { get; }
        public Guid RequestId { get; }
        public CreditCard CreditCard { get; }
        public Money Amount { get; }

        public PaymentRequested(Guid gatewayPaymentId, Guid requestId, CreditCard creditCard, Money amount)
        {
            GatewayPaymentId = gatewayPaymentId;
            RequestId = requestId;
            CreditCard = creditCard;
            Amount = amount;
        }
    }
}