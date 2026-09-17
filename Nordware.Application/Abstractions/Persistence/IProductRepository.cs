using Nordware.Domain.Entities;

namespace Nordware.Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken);
}