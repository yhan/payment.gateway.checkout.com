using System;

namespace PaymentGateway.Domain
{
    public class PaymentResult : IPaymentResult
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
        public PaymentStatus Status { get; set; }
        public Exception Exception { get; }
        public string Description { get; }

        public string Identifier
        {
            get => $"Payment: GatewayId={this.GatewayPaymentId}, RequestId={this.PaymentRequestId}";
            set{}
        }

        public static PaymentResult Fail(Guid gatewayPaymentId, Guid paymentRequestId, Exception exception, string description)
        {
            return new PaymentResult(gatewayPaymentId, paymentRequestId, PaymentStatus.Timeout, exception, description);
        }


        public static PaymentResult Finished(Guid gatewayPaymentId, Guid paymentRequestId)
        {
            return new PaymentResult(gatewayPaymentId, paymentRequestId, PaymentStatus.Success, null, null);
        }
    }

    public interface IPaymentResult
    {
        string Identifier { get; set; }
        PaymentStatus Status { get; set; }
    }

    public class WillHandleLaterPaymentResult : IPaymentResult
    {
        public Guid GatewayPaymentId { get; }
        public Guid PaymentRequestId { get; }
        public PaymentStatus Status { get; set; }
        public string Identifier
        {
            get => $"Payment: GatewayId={this.GatewayPaymentId}, RequestId={this.PaymentRequestId}";
            set{}
        }
       
        public WillHandleLaterPaymentResult(PaymentStatus status, Guid gatewayPaymentId, Guid requestPaymentId )
        {
            GatewayPaymentId = gatewayPaymentId;
            PaymentRequestId = requestPaymentId;
            Status = status;
        }
    }
}