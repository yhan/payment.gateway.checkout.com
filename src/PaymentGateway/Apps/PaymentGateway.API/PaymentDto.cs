using System;
using PaymentGateway.Domain;

namespace PaymentGateway.API
{
    public class PaymentDto
    {
        public Guid GateWayPaymentId { get; }
        public Guid AcquiringBankPaymentId { get; }
        public PaymentStatus Status { get; }
        public Guid RequestId { get; set; }

        public PaymentDto(Guid requestId, Guid gateWayPaymentId, Guid acquiringBankPaymentId,
            PaymentStatus status)
        {
            RequestId = requestId;
            GateWayPaymentId = gateWayPaymentId;
            AcquiringBankPaymentId = acquiringBankPaymentId;
            Status = status;
        }
    }
}