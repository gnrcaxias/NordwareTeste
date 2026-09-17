using MediatR;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Exceptions;
using Nordware.Application.Services;

namespace Nordware.Application.Commands;

public sealed class CancelReservationCommandHandler
    : IRequestHandler<CancelReservationCommand>
{
    private readonly IReservationRepository
        _reservationRepository;

    private readonly IProductRepository
        _productRepository;

    private readonly IReservationExpirationService
        _expirationService;

    public CancelReservationCommandHandler(
        IReservationRepository reservationRepository,
        IProductRepository productRepository,
        IReservationExpirationService expirationService)
    {
        _reservationRepository = reservationRepository;
        _productRepository = productRepository;
        _expirationService = expirationService;
    }

    public async Task Handle(
        CancelReservationCommand request,
        CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetActiveByProductIdAsync(request.ProductId, cancellationToken);

        if (reservation is null)
            throw new ReservationNotFoundException(request.ProductId);

        if (reservation.CustomerId != request.CustomerId)
            throw new ReservationNotOwnedException(request.ProductId, request.CustomerId);

        var expired =
            await _expirationService.ExpireIfNecessaryAsync(reservation, cancellationToken);

        if (expired)
            throw new ReservationExpiredException(request.ProductId);

        reservation.Cancel();

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        product?.Release();

        await _reservationRepository.SaveChangesAsync(cancellationToken);
    }
}
            