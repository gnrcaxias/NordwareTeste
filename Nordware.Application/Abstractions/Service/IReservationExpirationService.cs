using Nordware.Domain.Entities;

namespace Nordware.Application.Abstractions.Service;

public interface IReservationExpirationService
{

    Task<bool> ExpireIfNecessaryAsync(Reservation reservation, CancellationToken cancellationToken = default);

}