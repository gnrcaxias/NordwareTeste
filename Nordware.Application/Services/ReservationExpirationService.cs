using Nordware.Application.Abstractions.Persistence;
using Nordware.Domain.Entities;

namespace Nordware.Application.Services;

public sealed class ReservationExpirationService : IReservationExpirationService
{
    private readonly IProductRepository _productRepository;
    private readonly IReservationRepository _reservationRepository;

    public ReservationExpirationService(IProductRepository productRepository, IReservationRepository reservationRepository)
    {
        _productRepository = productRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<bool> ExpireIfNecessaryAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        if (!reservation.IsExpired(DateTime.UtcNow))
            return false;

        reservation.Expire();

        var product = await _productRepository.GetByIdAsync(reservation.ProductId, cancellationToken);

        product?.Release();

        await _reservationRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}