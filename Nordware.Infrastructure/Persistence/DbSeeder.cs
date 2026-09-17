using Microsoft.EntityFrameworkCore;
using Nordware.Domain.Entities;
using Nordware.Infrastructure.Persistence;

namespace ECommerce.Reservations.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Products.AnyAsync (cancellationToken)) return;

        db.Products.AddRange(
            new Product(new Guid(), "Notebook Pro 14"),
            new Product(new Guid(), "Smartphone X"),
            new Product(new Guid(), "Headset Wireless"),
            new Product(new Guid(), "Monitor 27"),
            new Product(new Guid(), "Teclado Mecânico"));

        await db.SaveChangesAsync(cancellationToken);
    }
}