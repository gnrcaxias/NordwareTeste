using Microsoft.EntityFrameworkCore;
using Nordware.Application.Abstractions.Persistence;

namespace Nordware.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}