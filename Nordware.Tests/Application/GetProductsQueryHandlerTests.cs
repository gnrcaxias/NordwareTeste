using FluentAssertions;
using NSubstitute;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Queries;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;

namespace Nordware.Tests.Application;

public sealed class GetProductsQueryHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();

        _handler = new GetProductsQueryHandler(
            _productRepository);
    }

    [Fact]
    public async Task Handle_WhenThereAreNoProducts_ShouldReturnEmptyCollection()
    {
        // Arrange
        _productRepository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(
            new GetProductsQuery(),
            CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new[]
        {
            new Product(
                Guid.NewGuid(),
                "Available Product",
                ProductStatus.Available),

            new Product(
                Guid.NewGuid(),
                "Reserved Product",
                ProductStatus.Reserved),

            new Product(
                Guid.NewGuid(),
                "Unavailable Product",
                ProductStatus.Unavailable)
        };

        _productRepository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(products);

        // Act
        var result = await _handler.Handle(
            new GetProductsQuery(),
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);

        result
            .Select(x => x.Status)
            .Should()
            .Contain([
                ProductStatus.Available.ToString(),
                ProductStatus.Reserved.ToString(),
                ProductStatus.Unavailable.ToString()
            ]);
    }
}