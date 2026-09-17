using MediatR;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.DTOs;

namespace Nordware.Application.Queries;

public sealed class GetCustomerReservationsQueryHandler
    : IRequestHandler<
        GetCustomerReservationsQuery,
        IReadOnlyList<ReservationResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IReservationRepository _reservationRepository;

    public GetCustomerReservationsQueryHandler(
        ICustomerRepository customerRepository,
        IReservationRepository reservationRepository)
    {
        _customerRepository = customerRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<IReadOnlyList<ReservationResponse>> Handle(GetCustomerReservationsQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            throw new KeyNotFoundException("Customer not found.");

        var reservations =
            await _reservationRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        return reservations
            .Select(ReservationResponse.FromEntity)
            .ToList();
    }
}