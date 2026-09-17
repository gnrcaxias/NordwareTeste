using Microsoft.EntityFrameworkCore;
using Nordware.Domain.Entities;

namespace Nordware.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync (cancellationToken)) 
            return;

        db.Products.AddRange(
            new Product(Guid.NewGuid(), "Notebook Pro 14"),
            new Product(Guid.NewGuid(), "Smartphone X"),
            new Product(Guid.NewGuid(), "Headset Wireless"),
            new Product(Guid.NewGuid(), "Monitor 27"),
            new Product(Guid.NewGuid(), "Teclado Mecânico"));

        db.Customers.AddRange(
            new Customer(Guid.NewGuid(), "Customer 1"),
            new Customer(Guid.NewGuid(), "Customer 2"),
            new Customer(Guid.NewGuid(), "Customer 3"));

        await db.SaveChangesAsync(cancellationToken);
    }
}