using FluentAssertions;
using NSubstitute;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Abstractions.Service;
using Nordware.Application.Commands;
using Nordware.Application.Exceptions;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;

namespace Nordware.Tests.Application;

public sealed class ReserveProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationExpirationService _expirationService;
    private readonly IUnitOfWork _unitOfWork;

    private readonly ReserveProductCommandHandler _handler;

    public ReserveProductCommandHandlerTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _customerRepository = Substitute.For<ICustomerRepository>();
        _reservationRepository = Substitute.For<IReservationRepository>();
        _expirationService = Substitute.For<IReservationExpirationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new ReserveProductCommandHandler(
            _productRepository,
            _customerRepository,
            _reservationRepository,
            _expirationService,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotExist_ShouldThrowCustomerNotFoundException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _customerRepository
            .GetByIdAsync(customerId, Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        var command = new ReserveProductCommand(
            productId,
            customerId);

        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<CustomerNotFoundException>();

        await _productRepository
            .DidNotReceive()
            .GetByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowProductNotFoundException()
    {
        // Arrange
        var customer = CreateCustomer();
        var productId = Guid.NewGuid();

        _customerRepository
            .GetByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(customer);

        _productRepository
            .GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new ReserveProductCommand(
            productId,
            customer.Id);

        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ProductNotFoundException>();

        await _reservationRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenActiveReservationExistsAndHasNotExpired_ShouldThrowProductUnavailableException()
    {
        // Arrange
        var customer = CreateCustomer();
        var product = CreateAvailableProduct();

        var reservation = CreateReservation(
            product.Id,
            customer.Id,
            DateTime.UtcNow);

        _customerRepository
            .GetByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(customer);

        _productRepository
            .GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        _reservationRepository
            .GetActiveByProductIdAsync(
                product.Id,
                Arg.Any<CancellationToken>())
            .Returns(reservation);

        _expirationService
            .ExpireIfNecessaryAsync(
                reservation,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new ReserveProductCommand(
            product.Id,
            customer.Id);

        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ProductUnavailableException>();

        product.Status.Should().Be(ProductStatus.Available);

        await _reservationRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>());

        await _unitOfWork
            .DidNotReceive()
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenActiveReservationHasExpired_ShouldCreateNewReservation()
    {
        // Arrange
        var customer = CreateCustomer();
        var product = CreateAvailableProduct();

        var expiredReservation = CreateReservation(
            product.Id,
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(-73));

        _customerRepository
            .GetByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(customer);

        _productRepository
            .GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        _reservationRepository
            .GetActiveByProductIdAsync(
                product.Id,
                Arg.Any<CancellationToken>())
            .Returns(expiredReservation);

        _expirationService
            .ExpireIfNecessaryAsync(
                expiredReservation,
                Arg.Any<CancellationToken>())
            .Returns(true);

        _unitOfWork
            .TrySaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(true);

        Reservation? addedReservation = null;

        _reservationRepository
            .When(x => x.AddAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                addedReservation = callInfo.Arg<Reservation>();
            });

        var command = new ReserveProductCommand(
            product.Id,
            customer.Id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(product.Id);
        result.CustomerId.Should().Be(customer.Id);
        result.Status.Should().Be(ReservationStatus.Active.ToString());

        product.Status.Should().Be(ProductStatus.Reserved);

        addedReservation.Should().NotBeNull();
        addedReservation!.ProductId.Should().Be(product.Id);
        addedReservation.CustomerId.Should().Be(customer.Id);
        addedReservation.Status.Should().Be(ReservationStatus.Active);

        await _unitOfWork
            .Received(1)
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductStatusIsNotAvailable_ShouldThrowProductUnavailableException()
    {
        // Arrange
        var customer = CreateCustomer();

        var product = new Product(
            Guid.NewGuid(),
            "Notebook",
            ProductStatus.Reserved);

        _customerRepository
            .GetByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(customer);

        _productRepository
            .GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        _reservationRepository
            .GetActiveByProductIdAsync(
                product.Id,
                Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        var command = new ReserveProductCommand(
            product.Id,
            customer.Id);

        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ProductUnavailableException>();

        await _reservationRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCommitFails_ShouldThrowProductUnavailableException()
    {
        // Arrange
        var customer = CreateCustomer();
        var product = CreateAvailableProduct();

        _customerRepository
            .GetByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(customer);

        _productRepository
            .GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        _reservationRepository
            .GetActiveByProductIdAsync(
                product.Id,
                Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        _unitOfWork
            .TrySaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new ReserveProductCommand(
            product.Id,
            customer.Id);

        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should()
            .ThrowAsync<ProductUnavailableException>();

        product.Status.Should().Be(ProductStatus.Reserved);

        await _unitOfWork
            .Received(1)
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEverythingIsValid_ShouldReserveProduct()
    {
        // Arrange
        var customer = CreateCustomer();
        var product = CreateAvailableProduct();

        _customerRepository
            .GetByIdAsync(customer.Id, Arg.Any<CancellationToken>())
            .Returns(customer);

        _productRepository
            .GetByIdAsync(product.Id, Arg.Any<CancellationToken>())
            .Returns(product);

        _reservationRepository
            .GetActiveByProductIdAsync(
                product.Id,
                Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        _unitOfWork
            .TrySaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(true);

        Reservation? addedReservation = null;

        _reservationRepository
            .When(x => x.AddAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                addedReservation = callInfo.Arg<Reservation>();
            });

        var command = new ReserveProductCommand(
            product.Id,
            customer.Id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(product.Id);
        result.CustomerId.Should().Be(customer.Id);
        result.Status.Should().Be(
            ReservationStatus.Active.ToString());

        result.ExpiresAt
            .Should()
            .BeCloseTo(
                result.CreatedAt.AddHours(72),
                TimeSpan.FromSeconds(1));

        product.Status.Should()
            .Be(ProductStatus.Reserved);

        addedReservation.Should().NotBeNull();

        await _reservationRepository
            .Received(1)
            .AddAsync(
                Arg.Any<Reservation>(),
                Arg.Any<CancellationToken>());

        await _unitOfWork
            .Received(1)
            .TrySaveChangesAsync(
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPropagateCancellationToken()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _customerRepository
            .GetByIdAsync(customerId, token)
            .Returns(Task.FromResult<Customer?>(null));

        var command = new ReserveProductCommand(
            productId,
            customerId);

        // Act
        var action = () => _handler.Handle(command, token);

        // Assert
        await action.Should()
            .ThrowAsync<CustomerNotFoundException>();

        await _customerRepository
            .Received(1)
            .GetByIdAsync(customerId, token);
    }

    private static Customer CreateCustomer()
    {
        return new Customer(
            Guid.NewGuid(),
            "Customer Test");
    }

    private static Product CreateAvailableProduct()
    {
        return new Product(
            Guid.NewGuid(),
            "Product Test",
            ProductStatus.Available);
    }

    private static Reservation CreateReservation(
        Guid productId,
        Guid customerId,
        DateTime createdAt)
    {
        return new Reservation(
            Guid.NewGuid(),
            productId,
            customerId,
            createdAt);
    }
}