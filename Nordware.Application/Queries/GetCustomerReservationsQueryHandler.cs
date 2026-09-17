using MediatR;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.DTOs;
using Nordware.Application.Exceptions;

namespace Nordware.Application.Queries;

public sealed class GetCustomerReservationsQueryHandler: IRequestHandler<GetCustomerReservationsQuery, IReadOnlyList<ReservationResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationExpirationService _expirationService;

    public GetCustomerReservationsQueryHandler(ICustomerRepository customerRepository, IReservationRepository reservationRepository, IReservationExpirationService expirationService)
    {
        _customerRepository = customerRepository;
        _reservationRepository = reservationRepository;
        _expirationService = expirationService;
    }

    public async Task<IReadOnlyList<ReservationResponse>> Handle(GetCustomerReservationsQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
            throw new CustomerNotFoundException(request.CustomerId);

        var reservations =
            await _reservationRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        foreach (var reservation in reservations)
        {
            await _expirationService.ExpireIfNecessaryAsync(reservation, cancellationToken);
        }

        return reservations
            .Select(ReservationResponse.FromEntity)
            .ToList();
    }
}