using System.Text.RegularExpressions;

namespace PaymentGateway.Domain
{
    public class Money
    {
        public string Currency { get; }
        public double Value { get; }

        public Money(string currency, double value)
        {
            Currency = currency;
            Value = value;
        }
        
        public bool IsValid(out string invalidReason)
        {
            invalidReason = null;
            if (Value <= 0)
            {
                invalidReason = "Amount should greater than 0";
                return false;
            }
            if(Currency == null || !Regex.IsMatch(Currency, "[A-Z]{3}"))
            {
                invalidReason = "Currency is absent or not correctly formatted";
                return false;
            }

            return true;
        }
    }
}   