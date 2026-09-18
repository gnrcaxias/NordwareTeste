using FluentAssertions;
using NSubstitute;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Commands;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;

namespace Nordware.Tests.Application;

public sealed class ExpireReservationsCommandHandlerTests
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ExpireReservationsCommandHandler _handler;

    public ExpireReservationsCommandHandlerTests()
    {
        _reservationRepository = Substitute.For<IReservationRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new ExpireReservationsCommandHandler(
            _reservationRepository,
            _productRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenThereAreNoExpiredReservations_ShouldNotSave()
    {
        // Arrange
        _reservationRepository
            .GetExpiredActiveAsync(
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await _handler.Handle(
            new ExpireReservationsCommand(),
            CancellationToken.None);

        // Assert
        await _unitOfWork
            .DidNotReceive()
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenThereIsExpiredReservation_ShouldExpireReservation()
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

        _reservationRepository
            .GetExpiredActiveAsync(
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns([reservation]);

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
        await _handler.Handle(
            new ExpireReservationsCommand(),
            CancellationToken.None);

        // Assert
        reservation.Status
            .Should()
            .Be(ReservationStatus.Expired);

        product.Status
            .Should()
            .Be(ProductStatus.Available);

        await _unitOfWork
            .Received(1)
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenMultipleReservationsAreExpired_ShouldExpireAllAndSaveOnce()
    {
        // Arrange
        var product1 = CreateReservedProduct();
        var product2 = CreateReservedProduct();
        var product3 = CreateReservedProduct();

        var reservation1 = CreateExpiredReservation(product1.Id);
        var reservation2 = CreateExpiredReservation(product2.Id);
        var reservation3 = CreateExpiredReservation(product3.Id);

        _reservationRepository
            .GetExpiredActiveAsync(
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>())
            .Returns([
                reservation1,
                reservation2,
                reservation3
            ]);

        _productRepository
            .GetByIdAsync(product1.Id, Arg.Any<CancellationToken>())
            .Returns(product1);

        _productRepository
            .GetByIdAsync(product2.Id, Arg.Any<CancellationToken>())
            .Returns(product2);

        _productRepository
            .GetByIdAsync(product3.Id, Arg.Any<CancellationToken>())
            .Returns(product3);

        _unitOfWork
            .TrySaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _handler.Handle(
            new ExpireReservationsCommand(),
            CancellationToken.None);

        // Assert
        reservation1.Status.Should().Be(ReservationStatus.Expired);
        reservation2.Status.Should().Be(ReservationStatus.Expired);
        reservation3.Status.Should().Be(ReservationStatus.Expired);

        product1.Status.Should().Be(ProductStatus.Available);
        product2.Status.Should().Be(ProductStatus.Available);
        product3.Status.Should().Be(ProductStatus.Available);

        await _unitOfWork
            .Received(1)
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    private static Product CreateReservedProduct()
    {
        return new Product(
            Guid.NewGuid(),
            "Product",
            ProductStatus.Reserved);
    }

    private static Reservation CreateExpiredReservation(
        Guid productId)
    {
        return new Reservation(
            Guid.NewGuid(),
            productId,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-73));
    }
}