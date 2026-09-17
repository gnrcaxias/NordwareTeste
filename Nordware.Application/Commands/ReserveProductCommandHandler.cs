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

    public ReserveProductCommandHandler(IProductRepository productRepository, ICustomerRepository customerRepository, IReservationRepository reservationRepository)
    {
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<ReservationResponse> Handle(ReserveProductCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            throw new CustomerNotFoundException(request.CustomerId);

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            throw new ProductNotFoundException(request.ProductId);

        if (product.Status != ProductStatus.Available)
            throw new ProductUnavailableException(request.ProductId);

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