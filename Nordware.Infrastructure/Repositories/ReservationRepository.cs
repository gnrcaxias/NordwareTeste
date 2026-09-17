using Nordware.Application.Abstractions.Persistence;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;
using Nordware.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Nordware.Infrastructure.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.Reservations.AddAsync(
            reservation,
            cancellationToken);
    }

    public async Task<Reservation?> GetActiveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .FirstOrDefaultAsync(
                x => x.ProductId == productId &&
                     x.Status == ReservationStatus.Active,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}