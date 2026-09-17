using Nordware.Domain.Entities;

namespace Nordware.Application.Abstractions.Persistence;

public interface IReservationExpirationService
{

    Task<bool> ExpireIfNecessaryAsync(Reservation reservation, CancellationToken cancellationToken = default);

}