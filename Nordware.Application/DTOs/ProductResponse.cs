using Nordware.Domain.Entities;

namespace Nordware.Application.DTOs;

public sealed record ProductResponse(Guid Id, string Name, string Status)
{
    public static ProductResponse FromEntity(Product product) =>
        new(product.Id, product.Name, product.Status.ToString());
}