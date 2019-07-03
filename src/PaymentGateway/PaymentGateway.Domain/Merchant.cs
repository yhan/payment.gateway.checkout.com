using System;

namespace PaymentGateway.Domain
{
    public class Merchant
    {
        public Guid Id { get; }
        public string Name { get; }

        public Merchant(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}