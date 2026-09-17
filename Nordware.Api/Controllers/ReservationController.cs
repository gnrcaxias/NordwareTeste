using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nordware.Application.Commands;
using Nordware.Application.DTOs;


namespace Nordware.Api.Controllers;

[ApiController]
[Route("products")]
public sealed class ReservationController(IMediator mediator) : ControllerBase
{
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    [HttpPost("{id:guid}/reserve")]
    public async Task<ActionResult<ReservationResponse>> Reserve(Guid id, [FromQuery] Guid customerId, CancellationToken cancellationToken)
    {
        var command = new ReserveProductCommand(id, customerId);

        var result = await mediator.Send(command, cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpDelete("{id:guid}/reserve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromQuery] Guid customerId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CancelReservationCommand(id, customerId), cancellationToken);

        return NoContent();
    }
}