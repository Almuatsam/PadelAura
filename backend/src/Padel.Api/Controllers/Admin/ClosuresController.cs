using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Padel.Application.Closures;

namespace Padel.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/closures")]
public sealed class ClosuresController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ClosureDto>>> GetAll(CancellationToken cancellationToken)
    {
        var closures = await sender.Send(new GetClosuresQuery(), cancellationToken);
        return Ok(closures);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateClosureCommand command, CancellationToken cancellationToken)
    {
        var ids = await sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { ids });
    }
}
