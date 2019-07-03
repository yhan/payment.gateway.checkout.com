using System;

namespace PaymentGateway.Infrastructure
{
    public struct MerchantDto
    {
        public Guid Id { get; }
        public string Name { get; }

        public MerchantDto(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}