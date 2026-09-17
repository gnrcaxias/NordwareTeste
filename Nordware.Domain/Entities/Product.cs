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

    public void Reserve()
    {
        if (Status != ProductStatus.Available)
            throw new InvalidOperationException(
                "Produto não está disponível para reserva.");

        Status = ProductStatus.Reserved;
    }

    public void Release()
    {
        Status = ProductStatus.Available;
    }

    public void MarkAsUnavailable()
    {
        Status = ProductStatus.Unavailable;
    }
}