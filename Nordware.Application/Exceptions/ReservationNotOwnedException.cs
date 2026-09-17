namespace Nordware.Application.Exceptions;

public sealed class ReservationNotOwnedException : Exception
{
    public ReservationNotOwnedException(Guid productId, Guid customerId)
        : base(
            $"A reserva do produto '{productId}' não pertence ao cliente '{customerId}'.")
    {
    }
}