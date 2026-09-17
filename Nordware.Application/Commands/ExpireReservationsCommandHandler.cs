using MediatR;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Domain.Enums;

namespace Nordware.Application.Commands;

public sealed class ExpireReservationsCommandHandler : IRequestHandler<ExpireReservationsCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExpireReservationsCommandHandler(IReservationRepository reservationRepository, IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ExpireReservationsCommand request, CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.GetExpiredActiveAsync(DateTime.UtcNow, cancellationToken);

        foreach (var reservation in reservations)
        {
            reservation.Expire();

            var product = await _productRepository.GetByIdAsync(reservation.ProductId, cancellationToken);

            product?.Release();
        }

        if (reservations.Count > 0)
            await _unitOfWork.TrySaveChangesAsync(cancellationToken);
    }
}