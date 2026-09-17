using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nordware.Application.Commands;

namespace Nordware.Api.Controllers;

[ApiController]
[Route("products")]
public sealed class ReservationController : ControllerBase
{
    private readonly ISender _sender;

    public ReservationController(ISender sender)
    {
        _sender = sender;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    [HttpPost("{id:guid}/reserve")]
    public async Task<IActionResult> Reserve(Guid id, [FromQuery] Guid customerId, CancellationToken cancellationToken)
    {
        var command = new ReserveProductCommand(id, customerId);

        var result = await _sender.Send(command, cancellationToken);

        return Ok(result);
    }
}