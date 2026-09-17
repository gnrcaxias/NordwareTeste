using Microsoft.EntityFrameworkCore;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Domain.Entities;
using Nordware.Infrastructure.Persistence;

namespace Nordware.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Customers
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}