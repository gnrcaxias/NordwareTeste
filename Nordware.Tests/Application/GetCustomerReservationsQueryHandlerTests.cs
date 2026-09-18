using FluentAssertions;
using NSubstitute;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Abstractions.Service;
using Nordware.Application.Exceptions;
using Nordware.Application.Queries;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;

namespace Nordware.Tests.Application;

public sealed class GetCustomerReservationsQueryHandlerTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationExpirationService _expirationService;

    private readonly GetCustomerReservationsQueryHandler _handler;

    public GetCustomerReservationsQueryHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();
        _reservationRepository = Substitute.For<IReservationRepository>();
        _expirationService = Substitute.For<IReservationExpirationService>();

        _handler = new GetCustomerReservationsQueryHandler(
            _customerRepository,
            _reservationRepository,
            _expirationService);
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotExist_ShouldThrow()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _customerRepository
            .GetByIdAsync(
                customerId,
                Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        // Act
        var action = () => _handler.Handle(
            new GetCustomerReservationsQuery(customerId),
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<CustomerNotFoundException>();

        await _reservationRepository
            .DidNotReceive()
            .GetByCustomerIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomerReservations()
    {
        // Arrange
        var customer = new Customer(
            Guid.NewGuid(),
            "Customer");

        var reservation1 = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            customer.Id,
            DateTime.UtcNow);

        var reservation2 = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            customer.Id,
            DateTime.UtcNow.AddHours(-1));

        _customerRepository
            .GetByIdAsync(
                customer.Id,
                Arg.Any<CancellationToken>())
            .Returns(customer);

        _reservationRepository
            .GetByCustomerIdAsync(
                customer.Id,
                Arg.Any<CancellationToken>())
            .Returns([
                reservation1,
                reservation2
            ]);

        _expirationService
            .ExpireIfNecessaryAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(
            new GetCustomerReservationsQuery(customer.Id),
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        result.Select(x => x.Id)
            .Should()
            .Contain([
                reservation1.Id,
                reservation2.Id
            ]);

        result
            .Should()
            .OnlyContain(x =>
                x.CustomerId == customer.Id);

        await _expirationService
            .Received(2)
            .ExpireIfNecessaryAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTryToExpireEachReservation()
    {
        // Arrange
        var customer = new Customer(
            Guid.NewGuid(),
            "Customer");

        var reservations = Enumerable
            .Range(1, 3)
            .Select(_ => new Reservation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                customer.Id,
                DateTime.UtcNow))
            .ToList();

        _customerRepository
            .GetByIdAsync(
                customer.Id,
                Arg.Any<CancellationToken>())
            .Returns(customer);

        _reservationRepository
            .GetByCustomerIdAsync(
                customer.Id,
                Arg.Any<CancellationToken>())
            .Returns(reservations);

        _expirationService
            .ExpireIfNecessaryAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        await _handler.Handle(
            new GetCustomerReservationsQuery(customer.Id),
            CancellationToken.None);

        // Assert
        foreach (var reservation in reservations)
        {
            await _expirationService
                .Received(1)
                .ExpireIfNecessaryAsync(
                    reservation,
                    Arg.Any<CancellationToken>());
        }
    }
}