using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nordware.Application.Queries;

namespace Nordware.Api.Controllers;

[ApiController]
[Route("customer")]
public sealed class CustomerController : ControllerBase
{
    private readonly ISender _sender;

    public CustomerController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomersQuery(), cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}/reservations")]
    public async Task<IActionResult> GetReservations(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerReservationsQuery(id);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result);
    }
}