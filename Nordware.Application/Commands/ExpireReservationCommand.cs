using MediatR;

namespace Nordware.Application.Commands;

public sealed record ExpireReservationsCommand: IRequest;