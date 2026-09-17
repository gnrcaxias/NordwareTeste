using Nordware.Domain.Entities;

namespace Nordware.Application.Abstractions.Persistence;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);

    Task<Reservation?> GetActiveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reservation>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reservation>> GetExpiredActiveAsync(DateTime now, CancellationToken cancellationToken = default);
}