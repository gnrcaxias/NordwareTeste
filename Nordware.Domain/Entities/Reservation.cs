using Nordware.Domain.Enums;

namespace Nordware.Domain.Entities;

public sealed class Reservation
{
    private Reservation()
    {
    }

    public Reservation(
        Guid id,
        Guid productId,
        Guid customerId,
        DateTime createdAt)
    {
        Id = id;
        ProductId = productId;
        CustomerId = customerId;
        CreatedAt = createdAt;
        ExpiresAt = createdAt.AddHours(72);
        Status = ReservationStatus.Active;
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public Guid CustomerId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public ReservationStatus Status { get; private set; }

    public bool IsExpired(DateTime now)
    {
        return Status == ReservationStatus.Active && ExpiresAt <= now;
    }

    public void Expire()
    {
        if (Status == ReservationStatus.Active)
            Status = ReservationStatus.Expired;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Active)
            Status = ReservationStatus.Cancelled;
    }
}