using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Padel.Application.Courts;
using Padel.Application.Courts.Commands.CreateCourt;
using Padel.Application.Courts.Commands.DeleteCourt;
using Padel.Application.Courts.Commands.UpdateCourt;
using Padel.Application.Courts.Queries.GetCourts;

namespace Padel.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/courts")]
public sealed class CourtsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CourtDto>>> GetAll(CancellationToken cancellationToken)
    {
        var courts = await sender.Send(new GetCourtsQuery(), cancellationToken);
        return Ok(courts);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateCourtCommand command, CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Created($"api/admin/courts/{id}", null);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<CourtDto>> Update(
        long id,
        UpdateCourtRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCourtCommand(id, request.Name, request.HourPrice, request.Status, request.Schedules);
        var court = await sender.Send(command, cancellationToken);
        return Ok(court);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCourtCommand(id), cancellationToken);
        return NoContent();
    }
}
