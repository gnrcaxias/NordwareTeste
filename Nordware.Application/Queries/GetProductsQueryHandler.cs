using MediatR;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.DTOs;

namespace Nordware.Application.Queries;

public sealed class GetProductsQueryHandler(IProductRepository productRepository): IRequestHandler<GetProductsQuery, IReadOnlyCollection<ProductResponse>>
{
    public async Task<IReadOnlyCollection<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        return products.Select(ProductResponse.FromEntity).ToList();
    }
}