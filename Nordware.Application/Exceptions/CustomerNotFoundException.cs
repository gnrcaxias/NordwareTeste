namespace Nordware.Application.Exceptions;

public sealed class CustomerNotFoundException : Exception
{
    public CustomerNotFoundException(Guid customerId)
        : base($"Cliente '{customerId}' não encontrado.")
    {
    }
}