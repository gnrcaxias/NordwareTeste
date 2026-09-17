using Microsoft.EntityFrameworkCore;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;
using Nordware.Infrastructure.Persistence;

namespace Nordware.Tests.Concurrency;

public sealed class ReserveProductConcurrencyTests
{
    [Fact]
    public async Task Two_contexts_updating_same_product_should_allow_only_one_commit()
    {
        var databaseName = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var productId = Guid.NewGuid();

        await using (var seedContext = new AppDbContext(options))
        {
            var product = new Product(
                productId,
                "Produto teste",
                ProductStatus.Available);

            seedContext.Products.Add(product);

            await seedContext.SaveChangesAsync();
        }

        await using var contextA = new AppDbContext(options);
        await using var contextB = new AppDbContext(options);

        var productA = await contextA.Products
            .SingleAsync(x => x.Id == productId);

        var productB = await contextB.Products
            .SingleAsync(x => x.Id == productId);

        productA.Reserve();
        productB.Reserve();

        await contextA.SaveChangesAsync();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => contextB.SaveChangesAsync());
    }
}