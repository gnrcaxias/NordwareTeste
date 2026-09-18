using FluentAssertions;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;

namespace Nordware.Tests.Domain;

public sealed class ReservationTests
{
    [Fact]
    public void Constructor_ShouldCreateActiveReservation()
    {
        // Arrange
        var createdAt = new DateTime(
            2026,
            1,
            1,
            10,
            0,
            0,
            DateTimeKind.Utc);

        // Act
        var reservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Active);
        reservation.CreatedAt.Should().Be(createdAt);
        reservation.ExpiresAt.Should().Be(createdAt.AddHours(72));
    }

    [Fact]
    public void IsExpired_BeforeExpiration_ShouldReturnFalse()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;

        var reservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        var now = createdAt.AddHours(71).AddMinutes(59);

        // Act
        var result = reservation.IsExpired(now);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ExactlyAtExpiration_ShouldReturnTrue()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;

        var reservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        var now = createdAt.AddHours(72);

        // Act
        var result = reservation.IsExpired(now);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_AfterExpiration_ShouldReturnTrue()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;

        var reservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        var now = createdAt.AddHours(72).AddSeconds(1);

        // Act
        var result = reservation.IsExpired(now);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenReservationIsCancelled_ShouldReturnFalse()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;

        var reservation = new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            createdAt);

        reservation.Cancel();

        // Act
        var result = reservation.IsExpired(
            createdAt.AddHours(100));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Expire_ShouldChangeStatusToExpired()
    {
        // Arrange
        var reservation = CreateReservation();

        // Act
        reservation.Expire();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Expired);
    }

    [Fact]
    public void Expire_WhenAlreadyCancelled_ShouldNotChangeStatus()
    {
        // Arrange
        var reservation = CreateReservation();

        reservation.Cancel();

        // Act
        reservation.Expire();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var reservation = CreateReservation();

        // Act
        reservation.Cancel();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyExpired_ShouldNotChangeStatus()
    {
        // Arrange
        var reservation = CreateReservation();

        reservation.Expire();

        // Act
        reservation.Cancel();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Expired);
    }

    private static Reservation CreateReservation()
    {
        return new Reservation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow);
    }
}