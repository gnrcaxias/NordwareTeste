namespace Nordware.Domain.Entities;

public sealed class Customer
{
    private Customer()
    {
    }

    public Customer(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;
}