using System.Text.RegularExpressions;
using PaymentGateway.Infrastructure;

namespace PaymentGateway.API.WriteAPI
{
    public class PaymentRequestValidator
    {
        private readonly PaymentRequest _paymentRequest;

        public PaymentRequestValidator(PaymentRequest paymentRequest)
        {
            _paymentRequest = paymentRequest;
        }

        public bool CardCvvInvalid()
        {
            var reg = "^[0-9]{3}$";
            return !Regex.IsMatch(_paymentRequest.Cvv, reg);
        }

        public  bool CardNumberInvalid()
        {
            var reg = "^[0-9]{4} [0-9]{4} [0-9]{4} [0-9]{4}$";
            return !Regex.IsMatch(_paymentRequest.CardNumber, reg);
        }

        public bool CardExpiryInvalid()
        {
            var reg = "^(0?[1-9]|1[012])/[0-9]{2}$";
            return !Regex.IsMatch(_paymentRequest.Expiry, reg);
        }
    }
}