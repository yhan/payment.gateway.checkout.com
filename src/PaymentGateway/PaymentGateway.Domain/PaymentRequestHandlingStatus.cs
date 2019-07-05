using System;

namespace PaymentGateway.Domain
{
    public class PaymentRequestHandlingStatus : IPaymentRequestHandlingResult
    {
        private PaymentRequestHandlingStatus(Guid gatewayPaymentId, Guid paymentRequestId, RequestHandlingStatus status, Exception exception,
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
        public RequestHandlingStatus Status { get; set; }
        public Exception Exception { get; }
        public string Description { get; }

        public string Identifier
        {
            get => $"Payment: GatewayId={this.GatewayPaymentId}, RequestId={this.PaymentRequestId}";
            set{}
        }

        public static PaymentRequestHandlingStatus Fail(Guid gatewayPaymentId, Guid paymentRequestId, Exception exception, string description)
        {
            return new PaymentRequestHandlingStatus(gatewayPaymentId, paymentRequestId, RequestHandlingStatus.Fail, exception, description);
        }


        public static PaymentRequestHandlingStatus Finished(Guid gatewayPaymentId, Guid paymentRequestId)
        {
            return new PaymentRequestHandlingStatus(gatewayPaymentId, paymentRequestId, RequestHandlingStatus.Success, null, null);
        }
    }

    public enum RequestHandlingStatus
    {
        Fail,
        Success
    }

    public interface IPaymentRequestHandlingResult
    {
        string Identifier { get; set; }
        RequestHandlingStatus Status { get; set; }
    }
}