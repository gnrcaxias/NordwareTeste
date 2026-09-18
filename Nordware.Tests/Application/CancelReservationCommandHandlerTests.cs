using FluentAssertions;
using NSubstitute;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Abstractions.Service;
using Nordware.Application.Commands;
using Nordware.Application.Exceptions;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;

namespace Nordware.Tests.Application;

public sealed class CancelReservationCommandHandlerTests
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IProductRepository _productRepository;
    private readonly IReservationExpirationService _expirationService;
    private readonly IUnitOfWork _unitOfWork;

    private readonly CancelReservationCommandHandler _handler;

    public CancelReservationCommandHandlerTests()
    {
        _reservationRepository = Substitute.For<IReservationRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _expirationService = Substitute.For<IReservationExpirationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CancelReservationCommandHandler(
            _reservationRepository,
            _productRepository,
            _expirationService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenReservationDoesNotExist_ShouldThrowReservationNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        _reservationRepository
            .GetActiveByProductIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        var command = new CancelReservationCommand(
            productId,
            customerId);

        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ReservationNotFoundException>();

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
    public async Task Handle_WhenReservationBelongsToAnotherCustomer_ShouldThrowReservationNotOwnedException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var anotherCustomerId = Guid.NewGuid();

        var reservation = CreateReservation(
            productId,
            ownerId);

        _reservationRepository
            .GetActiveByProductIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        var command = new CancelReservationCommand(
            productId,
            anotherCustomerId);

        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ReservationNotOwnedException>();

        reservation.Status
            .Should()
            .Be(ReservationStatus.Active);

        await _expirationService
            .DidNotReceive()
            .ExpireIfNecessaryAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>());

        await _unitOfWork
            .DidNotReceive()
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReservationExpired_ShouldThrowReservationExpiredException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var reservation = CreateReservation(
            productId,
            customerId);

        _reservationRepository
            .GetActiveByProductIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        _expirationService
            .ExpireIfNecessaryAsync(
                reservation,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CancelReservationCommand(
            productId,
            customerId);

        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ReservationExpiredException>();

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
    public async Task Handle_WhenReservationIsActive_ShouldCancelAndReleaseProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var reservation = CreateReservation(
            productId,
            customerId);

        var product = new Product(
            productId,
            "Notebook",
            ProductStatus.Reserved);

        _reservationRepository
            .GetActiveByProductIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        _expirationService
            .ExpireIfNecessaryAsync(
                reservation,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _productRepository
            .GetByIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns(product);

        _unitOfWork
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CancelReservationCommand(
            productId,
            customerId);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        reservation.Status
            .Should()
            .Be(ReservationStatus.Cancelled);

        product.Status
            .Should()
            .Be(ProductStatus.Available);

        await _unitOfWork
            .Received(1)
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldStillCancelReservation()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var reservation = CreateReservation(
            productId,
            customerId);

        _reservationRepository
            .GetActiveByProductIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        _expirationService
            .ExpireIfNecessaryAsync(
                reservation,
                Arg.Any<CancellationToken>())
            .Returns(false);

        _productRepository
            .GetByIdAsync(
                productId,
                Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        _unitOfWork
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CancelReservationCommand(
            productId,
            customerId);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        reservation.Status
            .Should()
            .Be(ReservationStatus.Cancelled);

        await _unitOfWork
            .Received(1)
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    private static Reservation CreateReservation(
        Guid productId,
        Guid customerId)
    {
        return new Reservation(
            Guid.NewGuid(),
            productId,
            customerId,
            DateTime.UtcNow);
    }
}