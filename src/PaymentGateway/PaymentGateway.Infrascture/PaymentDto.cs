using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentDto
    {
        public PaymentDto(Guid requestId, Guid gatewayPaymentId, Guid? acquiringBankPaymentId, PaymentStatus status, bool? approved)
        {
            RequestId = requestId;
            GatewayPaymentId = gatewayPaymentId;
            AcquiringBankPaymentId = acquiringBankPaymentId;
            Status = status;
            Approved = approved;
        }

        public Guid GatewayPaymentId { get; }

        public Guid? AcquiringBankPaymentId { get; }

        public PaymentStatus Status { get; }

        public Guid RequestId { get; set; }

        public bool? Approved { get; }
    }
}