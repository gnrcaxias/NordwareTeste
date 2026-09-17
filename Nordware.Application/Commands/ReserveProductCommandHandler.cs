using MediatR;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.DTOs;
using Nordware.Application.Exceptions;
using Nordware.Domain.Entities;
using Nordware.Domain.Enums;

namespace Nordware.Application.Commands;

public sealed class ReserveProductCommandHandler: IRequestHandler<ReserveProductCommand, ReservationResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationExpirationService _expirationService;

    public ReserveProductCommandHandler(IProductRepository productRepository, 
                                        ICustomerRepository customerRepository, 
                                        IReservationRepository reservationRepository, 
                                        IReservationExpirationService expirationService)
    {
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _reservationRepository = reservationRepository;
        _expirationService = expirationService;
    }

    public async Task<ReservationResponse> Handle(ReserveProductCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            throw new CustomerNotFoundException(request.CustomerId);

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.ProductId);

        var activeReservation =
            await _reservationRepository.GetActiveByProductIdAsync(request.ProductId, cancellationToken);

        if (activeReservation is not null)
        {
            var expired =
                await _expirationService.ExpireIfNecessaryAsync(activeReservation, cancellationToken);

            if (!expired)
                throw new ProductUnavailableException(request.ProductId);
        }

        product.Reserve();

        var now = DateTime.UtcNow;

        var reservation = new Reservation(
            Guid.NewGuid(),
            product.Id,
            customer.Id,
            now);

        await _reservationRepository.AddAsync(
            reservation,
            cancellationToken);

        await _reservationRepository.SaveChangesAsync(
            cancellationToken);

        return ReservationResponse.FromEntity(reservation);
    }
}