using Nordware.Domain.Entities;

namespace Nordware.Application.DTOs;

public sealed record CustomerResponse(Guid Id, string Name)
{
    public static CustomerResponse FromEntity(Customer customer)
    {
        return new(
            customer.Id,
            customer.Name);
    }
}