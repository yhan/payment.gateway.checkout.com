using System;
using PaymentGateway.Domain;

namespace PaymentGateway.Infrastructure
{
    public class PaymentDto
    {
        public Guid GatewayPaymentId { get; }
        public Guid AcquiringBankPaymentId { get; }
        public PaymentStatus Status { get; }
        public Guid RequestId { get; set; }

        public bool Approved => Status == PaymentStatus.Success;

        public PaymentDto(Guid requestId, Guid gatewayPaymentId, Guid acquiringBankPaymentId,
            PaymentStatus status)
        {
            RequestId = requestId;
            GatewayPaymentId = gatewayPaymentId;
            AcquiringBankPaymentId = acquiringBankPaymentId;
            Status = status;
        }
    }
}