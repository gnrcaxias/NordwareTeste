namespace Nordware.Application.Exceptions;

public sealed class ReservationNotFoundException : Exception
{
    public ReservationNotFoundException(Guid productId)
        : base($"Nenhuma reserva ativa encontrada para o produto '{productId}'.")
    {
    }
}