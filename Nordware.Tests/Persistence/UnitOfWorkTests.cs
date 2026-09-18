using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Nordware.Infrastructure.Persistence;

namespace Nordware.Tests.Persistence;

public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task TrySaveChangesAsync_should_return_true_when_save_succeeds()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);

        var unitOfWork = new UnitOfWork(context);

        // Act
        var result = await unitOfWork.TrySaveChangesAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TrySaveChangesAsync_should_return_false_when_concurrency_exception_occurs()
    {
        // Arrange
        var context = Substitute.For<AppDbContext>(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        context
            .SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns<Task<int>>(
                _ => throw new DbUpdateConcurrencyException());

        var unitOfWork = new UnitOfWork(context);

        // Act
        var result = await unitOfWork.TrySaveChangesAsync();

        // Assert
        result.Should().BeFalse();

        await context
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}