using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;
using Nordware.Infrastructure.Persistence;

namespace Nordware.Tests.Concurrency;

public sealed class ReserveProductConcurrencyTests
{
    [Fact]
    public async Task Two_contexts_should_load_independent_product_snapshots()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var options = CreateOptions(databaseName);

        var productId = Guid.NewGuid();

        await using (var seedContext = new AppDbContext(options))
        {
            seedContext.Products.Add(
                new Product(
                    productId,
                    "Notebook",
                    ProductStatus.Available));

            await seedContext.SaveChangesAsync();
        }

        await using var contextA = new AppDbContext(options);
        await using var contextB = new AppDbContext(options);

        var productA = await contextA.Products
            .SingleAsync(x => x.Id == productId);

        var productB = await contextB.Products
            .SingleAsync(x => x.Id == productId);

        // Act
        productA.Reserve();

        // Assert
        productA.Status.Should()
            .Be(ProductStatus.Reserved);

        productB.Status.Should()
            .Be(ProductStatus.Available);

        productA.Should()
            .NotBeSameAs(productB);
    }

    [Fact]
    public async Task After_first_context_commits_second_context_should_still_have_old_snapshot()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var options = CreateOptions(databaseName);

        var productId = Guid.NewGuid();

        await using (var seedContext = new AppDbContext(options))
        {
            seedContext.Products.Add(
                new Product(
                    productId,
                    "Notebook",
                    ProductStatus.Available));

            await seedContext.SaveChangesAsync();
        }

        await using var contextA = new AppDbContext(options);
        await using var contextB = new AppDbContext(options);

        var productA = await contextA.Products
            .SingleAsync(x => x.Id == productId);

        var productB = await contextB.Products
            .SingleAsync(x => x.Id == productId);

        // Act
        productA.Reserve();

        await contextA.SaveChangesAsync();

        // Assert
        productA.Status
            .Should()
            .Be(ProductStatus.Reserved);

        productB.Status
            .Should()
            .Be(ProductStatus.Available);

        var persistedProduct = await contextB.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == productId);

        persistedProduct.Status
            .Should()
            .Be(ProductStatus.Reserved);

        contextB.Entry(productB)
            .Property(x => x.Status)
            .OriginalValue
            .Should()
            .Be(ProductStatus.Available);
    }

    [Fact]
    public async Task After_first_context_commits_new_context_should_read_updated_value()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        var options = CreateOptions(databaseName);

        var productId = Guid.NewGuid();

        await using (var seedContext = new AppDbContext(options))
        {
            seedContext.Products.Add(
                new Product(
                    productId,
                    "Notebook",
                    ProductStatus.Available));

            await seedContext.SaveChangesAsync();
        }

        await using var contextA = new AppDbContext(options);

        var productA = await contextA.Products
            .SingleAsync(x => x.Id == productId);

        // Act
        productA.Reserve();

        await contextA.SaveChangesAsync();

        await using var contextB = new AppDbContext(options);

        var productB = await contextB.Products
            .SingleAsync(x => x.Id == productId);

        // Assert
        productB.Status
            .Should()
            .Be(ProductStatus.Reserved);
    }

    private static DbContextOptions<AppDbContext> CreateOptions(
        string databaseName)
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }
}