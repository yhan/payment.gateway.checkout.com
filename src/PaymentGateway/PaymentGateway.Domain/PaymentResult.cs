using System;

namespace PaymentGateway.Domain
{
    public class PaymentResult
    {
        private PaymentResult(Guid gatewayPaymentId, Guid paymentRequestId, PaymentStatus status, Exception exception,
            string description)
        {
            GatewayPaymentId = gatewayPaymentId;
            PaymentRequestId = paymentRequestId;
            Status = status;
            Exception = exception;
            Description = description;
        }

        public Guid GatewayPaymentId { get; }
        public Guid PaymentRequestId { get; }
        public PaymentStatus Status { get; }
        public Exception Exception { get; }
        public string Description { get; }

        public string Identifier => $"Payment: GatewayId={this.GatewayPaymentId}, RequestId={this.PaymentRequestId}";

        public static PaymentResult Fail(Guid gatewayPaymentId, Guid paymentRequestId, Exception exception, string description)
        {
            return new PaymentResult(gatewayPaymentId, paymentRequestId, PaymentStatus.Timeout, exception, description);
        }


        public static PaymentResult Finished(Guid gatewayPaymentId, Guid paymentRequestId)
        {
            return new PaymentResult(gatewayPaymentId, paymentRequestId, PaymentStatus.Success, null, null);
        }
    }
}