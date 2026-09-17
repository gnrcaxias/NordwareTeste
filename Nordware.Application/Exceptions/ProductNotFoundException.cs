namespace Nordware.Application.Exceptions;

public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId)
        : base($"Produto '{productId}' não encontrado.")
    {
    }
}