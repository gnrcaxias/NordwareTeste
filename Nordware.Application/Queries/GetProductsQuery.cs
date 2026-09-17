using MediatR;
using Nordware.Application.DTOs;

namespace Nordware.Application.Queries;

public sealed record GetProductsQuery : IRequest<IReadOnlyCollection<ProductResponse>>;