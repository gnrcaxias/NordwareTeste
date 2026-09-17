namespace Nordware.Application.Exceptions;

public sealed class ProductUnavailableException : Exception
{
    public ProductUnavailableException(Guid productId)
        : base($"Produto '{productId}' não está disponível.")
    {
    }
}