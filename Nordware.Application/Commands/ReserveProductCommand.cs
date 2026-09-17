using MediatR;
using Nordware.Application.DTOs;

namespace Nordware.Application.Commands;

public sealed record ReserveProductCommand(
    Guid ProductId,
    Guid CustomerId) : IRequest<ReservationResponse>;