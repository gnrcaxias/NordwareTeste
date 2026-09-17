using MediatR;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.DTOs;

namespace Nordware.Application.Queries;

public sealed class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, IReadOnlyList<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IReadOnlyList<CustomerResponse>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);

        return customers
            .Select(CustomerResponse.FromEntity)
            .ToList();
    }
}