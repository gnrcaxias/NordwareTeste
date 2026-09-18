using FluentAssertions;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;

namespace Nordware.Tests.Domain;

public sealed class ProductTests
{
    [Fact]
    public void Constructor_ShouldCreateAvailableProductByDefault()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var product = new Product(id, "Notebook");

        // Assert
        product.Id.Should().Be(id);
        product.Name.Should().Be("Notebook");
        product.Status.Should().Be(ProductStatus.Available);
    }

    [Fact]
    public void Reserve_ShouldChangeStatusToReserved()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "Notebook",
            ProductStatus.Available);

        // Act
        product.Reserve();

        // Assert
        product.Status.Should().Be(ProductStatus.Reserved);
    }

    [Fact]
    public void Reserve_WhenProductIsAlreadyReserved_ShouldThrow()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "Notebook",
            ProductStatus.Reserved);

        // Act
        var action = () => product.Reserve();

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Produto não está disponível para reserva.");
    }

    [Fact]
    public void Reserve_WhenProductIsUnavailable_ShouldThrow()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "Notebook",
            ProductStatus.Unavailable);

        // Act
        var action = () => product.Reserve();

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Produto não está disponível para reserva.");
    }

    [Fact]
    public void Release_ShouldMakeProductAvailable()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "Notebook",
            ProductStatus.Reserved);

        // Act
        product.Release();

        // Assert
        product.Status.Should().Be(ProductStatus.Available);
    }

    [Fact]
    public void Release_WhenProductIsUnavailable_ShouldMakeItAvailable()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "Notebook",
            ProductStatus.Unavailable);

        // Act
        product.Release();

        // Assert
        product.Status.Should().Be(ProductStatus.Available);
    }

    [Fact]
    public void MarkAsUnavailable_ShouldChangeStatus()
    {
        // Arrange
        var product = new Product(
            Guid.NewGuid(),
            "Notebook",
            ProductStatus.Available);

        // Act
        product.MarkAsUnavailable();

        // Assert
        product.Status.Should().Be(ProductStatus.Unavailable);
    }
}
