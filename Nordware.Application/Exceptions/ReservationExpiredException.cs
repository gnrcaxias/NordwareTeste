namespace Nordware.Application.Exceptions;

public sealed class ReservationExpiredException : Exception
{
    public ReservationExpiredException(Guid productId)
        : base($"A reserva do produto '{productId}' já expirou.")
    {
    }
}