using MediatR;

namespace Nordware.Application.Commands;

public sealed record CancelReservationCommand(Guid ProductId, Guid CustomerId) : IRequest;