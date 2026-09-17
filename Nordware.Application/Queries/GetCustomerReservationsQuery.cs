using MediatR;
using Nordware.Application.DTOs;

namespace Nordware.Application.Queries;

public sealed record GetCustomerReservationsQuery(Guid CustomerId) : IRequest<IReadOnlyList<ReservationResponse>>;