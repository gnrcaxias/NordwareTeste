using Nordware.Domain.Enums;

namespace Nordware.Domain.Entities;

public sealed class Product
{
    private Product() { }

    public Product(Guid id, string name, ProductStatus status = ProductStatus.Available)
    {
        Id = id;
        Name = name;
        Status = status;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public ProductStatus Status { get; private set; }
}