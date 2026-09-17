using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Abstractions.Service;
using Nordware.Domain.Entities;

namespace Nordware.Application.Services;

public sealed class ReservationExpirationService : IReservationExpirationService
{
    private readonly IProductRepository _productRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;


    public ReservationExpirationService(IProductRepository productRepository, IReservationRepository reservationRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ExpireIfNecessaryAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        if (!reservation.IsExpired(DateTime.UtcNow))
            return false;

        reservation.Expire();

        var product = await _productRepository.GetByIdAsync(reservation.ProductId, cancellationToken);

        product?.Release();

        var committed = await _unitOfWork.TrySaveChangesAsync(cancellationToken);

        return true;
    }
}