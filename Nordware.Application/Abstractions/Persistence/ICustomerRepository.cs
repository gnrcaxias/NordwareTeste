using Nordware.Domain.Entities;

namespace Nordware.Application.Abstractions.Persistence;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}