using Nordware.Domain.Entities;

namespace Nordware.Application.DTOs;

public sealed record ReservationResponse(
    Guid Id,
    Guid ProductId,
    Guid CustomerId,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    string Status)
{
    public static ReservationResponse FromEntity(
        Reservation reservation)
    {
        return new(
            reservation.Id,
            reservation.ProductId,
            reservation.CustomerId,
            reservation.CreatedAt,
            reservation.ExpiresAt,
            reservation.Status.ToString());
    }
}