using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nordware.Application.Commands;
using Nordware.Application.Services;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;
using Nordware.Infrastructure.Persistence;
using Nordware.Infrastructure.Repositories;

namespace Nordware.Tests.Integration;

public sealed class ReReservationAfterExpirationTests
{
    [Fact]
    public async Task Product_should_be_reservable_after_previous_reservation_expires()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var productId = Guid.NewGuid();
        var customerAId = Guid.NewGuid();
        var customerBId = Guid.NewGuid();

        await using (var context = new AppDbContext(options))
        {
            var productT = new Product(
                productId,
                "Notebook");

            productT.Reserve();

            context.Products.Add(productT);

            context.Customers.AddRange(
                new Customer(customerAId, "Customer A"),
                new Customer(customerBId, "Customer B"));

            context.Reservations.Add(
                new Reservation(
                    Guid.NewGuid(),
                    productId,
                    customerAId,
                    DateTime.UtcNow.AddHours(-73)));

            await context.SaveChangesAsync();
        }

        // Expire previous reservation
        await using (var expirationContext =
            new AppDbContext(options))
        {
            var expirationHandler =
                new ExpireReservationsCommandHandler(
                    new ReservationRepository(expirationContext),
                    new ProductRepository(expirationContext),
                    new UnitOfWork(expirationContext));

            await expirationHandler.Handle(
                new ExpireReservationsCommand(),
                CancellationToken.None);
        }

        // Act - Customer B reserves
        await using var reserveContext =
            new AppDbContext(options);

        var expirationService =
            new ReservationExpirationService(
                new ProductRepository(reserveContext),
                new ReservationRepository(reserveContext),
                new UnitOfWork(reserveContext));

        var reserveHandler =
            new ReserveProductCommandHandler(
                new ProductRepository(reserveContext),
                new CustomerRepository(reserveContext),
                new ReservationRepository(reserveContext),
                expirationService,
                new UnitOfWork(reserveContext));

        var result = await reserveHandler.Handle(
            new ReserveProductCommand(
                productId,
                customerBId),
            CancellationToken.None);

        // Assert
        result.CustomerId.Should().Be(customerBId);
        result.ProductId.Should().Be(productId);
        result.Status.Should().Be("Active");

        var product = await reserveContext.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == productId);

        product.Status.Should().Be(ProductStatus.Reserved);
    }
}
