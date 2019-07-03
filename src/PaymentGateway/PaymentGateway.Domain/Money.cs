using System.Collections.Generic;
using System.Linq;
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
        
        public bool IsValid(out List<string> invalidReason)
        {
            invalidReason = new List<string>();
            if (Value <= 0)
            {
                invalidReason.Add("Amount should be greater than 0");
            }
            if(Currency == null || !Regex.IsMatch(Currency, "[A-Z]{3}"))
            {
                invalidReason.Add("Currency is absent or not correctly formatted");
            }

            return !invalidReason.Any();
        }
    }
}   