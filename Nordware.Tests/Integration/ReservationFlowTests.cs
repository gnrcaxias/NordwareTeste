using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nordware.Application.Commands;
using Nordware.Application.Services;
using Nordware.Infrastructure.Persistence;
using Nordware.Infrastructure.Repositories;

namespace Nordware.Tests.Integration;

public sealed class ReservationFlowTests
{
    [Fact]
    public async Task Reserve_and_cancel_should_restore_product_availability()
    {
        // Arrange
        var options = CreateOptions();

        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        await using (var context = new AppDbContext(options))
        {
            context.Customers.Add(
                new Nordware.Domain.Entities.Customer(
                    customerId,
                    "Customer"));

            context.Products.Add(
                new Nordware.Domain.Entities.Product(
                    productId,
                    "Notebook"));

            await context.SaveChangesAsync();
        }

        await using var reserveContext =
            new AppDbContext(options);

        var reserveHandler = CreateReserveHandler(
            reserveContext);

        // Act - Reserve
        var reservation = await reserveHandler.Handle(
            new ReserveProductCommand(
                productId,
                customerId),
            CancellationToken.None);

        // Assert
        reservation.ProductId.Should().Be(productId);
        reservation.CustomerId.Should().Be(customerId);
        reservation.Status.Should().Be("Active");

        var reservedProduct = await reserveContext.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == productId);

        reservedProduct.Status
            .Should()
            .Be(Nordware.Domain.Enums.ProductStatus.Reserved);

        // Arrange - Cancel
        await using var cancelContext =
            new AppDbContext(options);

        var cancelHandler = CreateCancelHandler(
            cancelContext);

        // Act - Cancel
        await cancelHandler.Handle(
            new CancelReservationCommand(
                productId,
                customerId),
            CancellationToken.None);

        // Assert
        var finalProduct = await cancelContext.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == productId);

        finalProduct.Status
            .Should()
            .Be(Nordware.Domain.Enums.ProductStatus.Available);

        var finalReservation = await cancelContext.Reservations
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);

        finalReservation.Status
            .Should()
            .Be(Nordware.Domain.Enums.ReservationStatus.Cancelled);
    }

    [Fact]
    public async Task Expired_reservation_should_release_product()
    {
        // Arrange
        var options = CreateOptions();

        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        await using (var context = new AppDbContext(options))
        {
            var customer =
                new Nordware.Domain.Entities.Customer(
                    customerId,
                    "Customer");

            var productT =
                new Nordware.Domain.Entities.Product(
                    productId,
                    "Notebook");

            var reservationT =
                new Nordware.Domain.Entities.Reservation(
                    Guid.NewGuid(),
                    productId,
                    customerId,
                    DateTime.UtcNow.AddHours(-73));

            productT.Reserve();

            context.Customers.Add(customer);
            context.Products.Add(productT);
            context.Reservations.Add(reservationT);

            await context.SaveChangesAsync();
        }

        await using var context2 =
            new AppDbContext(options);

        var handler = new Nordware.Application.Commands
            .ExpireReservationsCommandHandler(
                new ReservationRepository(context2),
                new ProductRepository(context2),
                new UnitOfWork(context2));

        // Act
        await handler.Handle(
            new ExpireReservationsCommand(),
            CancellationToken.None);

        // Assert
        var product = await context2.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == productId);

        product.Status
            .Should()
            .Be(Nordware.Domain.Enums.ProductStatus.Available);

        var reservation = await context2.Reservations
            .AsNoTracking()
            .SingleAsync(x => x.ProductId == productId);

        reservation.Status
            .Should()
            .Be(Nordware.Domain.Enums.ReservationStatus.Expired);
    }

    private static AppDbContext CreateContext(
        DbContextOptions<AppDbContext> options)
    {
        return new AppDbContext(options);
    }

    private static Nordware.Application.Commands.ReserveProductCommandHandler
        CreateReserveHandler(AppDbContext context)
    {
        return new Nordware.Application.Commands
            .ReserveProductCommandHandler(
                new ProductRepository(context),
                new CustomerRepository(context),
                new ReservationRepository(context),
                new ReservationExpirationService(
                    new ProductRepository(context),
                    new ReservationRepository(context),
                    new UnitOfWork(context)),
                new UnitOfWork(context));
    }

    private static Nordware.Application.Commands.CancelReservationCommandHandler
        CreateCancelHandler(AppDbContext context)
    {
        return new Nordware.Application.Commands
            .CancelReservationCommandHandler(
                new ReservationRepository(context),
                new ProductRepository(context),
                new ReservationExpirationService(
                    new ProductRepository(context),
                    new ReservationRepository(context),
                    new UnitOfWork(context)),
                new UnitOfWork(context));
    }

    private static DbContextOptions<AppDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }
}