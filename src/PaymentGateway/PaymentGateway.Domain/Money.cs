namespace PaymentGateway.Domain
{
    public struct Money
    {
        public string Currency { get; }
        public double Value { get; }

        public Money(string currency, double value)
        {
            Currency = currency;
            Value = value;
        }
    }
}   