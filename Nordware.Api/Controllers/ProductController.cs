using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nordware.Application.DTOs;
using Nordware.Application.Queries;

namespace Nordware.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ProductResponse>>> GetProducts(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProductsQuery(), cancellationToken));
    }
}