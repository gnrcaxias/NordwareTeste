using FluentAssertions;
using NSubstitute;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Services;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;

namespace Nordware.Tests.Application;

public sealed class ReservationExpirationServiceTests
{
    private readonly IProductRepository _productRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ReservationExpirationService _service;

    public ReservationExpirationServiceTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _reservationRepository = Substitute.For<IReservationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _service = new ReservationExpirationService(
            _productRepository,
            _reservationRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task ExpireIfNecessary_WhenReservationIsStillActive_ShouldReturnFalse()
    {
        // Arrange
        var reservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow);

        // Act
        var result = await _service.ExpireIfNecessaryAsync(
            reservation,
            CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        reservation.Status
            .Should()
            .Be(ReservationStatus.Active);

        await _productRepository
            .DidNotReceive()
            .GetByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await _unitOfWork
            .DidNotReceive()
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExpireIfNecessary_WhenReservationIsExpired_ShouldExpireReservation()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var reservation = new Reservation(
            Guid.NewGuid(),
            productId,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-73));

        var product = new Product(
            productId,
            "Notebook",
            ProductStatus.Reserved);

        _productRepository
            .GetByIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns(product);

        _unitOfWork
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _service.ExpireIfNecessaryAsync(
            reservation,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        reservation.Status
            .Should()
            .Be(ReservationStatus.Expired);

        product.Status
            .Should()
            .Be(ProductStatus.Available);

        await _productRepository
            .Received(1)
            .GetByIdAsync(
                productId,
                Arg.Any<CancellationToken>());

        await _unitOfWork
            .Received(1)
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExpireIfNecessary_WhenProductDoesNotExist_ShouldExpireReservation()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var reservation = new Reservation(
            Guid.NewGuid(),
            productId,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-73));

        _productRepository
            .GetByIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        _unitOfWork
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _service.ExpireIfNecessaryAsync(
            reservation,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        reservation.Status
            .Should()
            .Be(ReservationStatus.Expired);

        await _unitOfWork
            .Received(1)
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExpireIfNecessary_WhenCommitFails_ShouldReturnFalse()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var reservation = new Reservation(
            Guid.NewGuid(),
            productId,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-73));

        var product = new Product(
            productId,
            "Notebook",
            ProductStatus.Reserved);

        _productRepository
            .GetByIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns(product);

        _unitOfWork
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _service.ExpireIfNecessaryAsync(
            reservation,
            CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}