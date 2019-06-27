namespace PaymentGateway.Domain
{
    public struct Money
    {
        public string Currency { get; }
        public double Amount { get; }

        public Money(string currency, double amount)
        {
            Currency = currency;
            Amount = amount;
        }
    }
}   