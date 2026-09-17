namespace Nordware.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken = default);
}