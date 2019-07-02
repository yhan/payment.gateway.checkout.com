using System;
using SimpleCQRS;

namespace PaymentGateway.Domain
{
    public class RequestPaymentCommand : Command
    {
 
        public Guid GatewayPaymentId { get; }
        public Guid RequestId { get; }
        public CreditCard CreditCard { get; }
        public Money Amount { get; }
        public Guid MerchantId { get; set; }

        public RequestPaymentCommand(Guid gatewayPaymentId, Guid merchantId, Guid requestId, CreditCard creditCard, Money amount)
        {
            GatewayPaymentId = gatewayPaymentId;
            MerchantId = merchantId;
            RequestId = requestId;
            CreditCard = creditCard;
            Amount = amount;
        }
    }

    public class CreditCard
    {
        public string Number { get; }
        public string Cvv { get; }
        public string Expiry { get; }
        public string HolderName { get; }

        public CreditCard( string number, string cvv, string expiry, string holderName)
        {
            Number = number;
            Cvv = cvv;
            Expiry = expiry;
            HolderName = holderName;
        }
    }
}