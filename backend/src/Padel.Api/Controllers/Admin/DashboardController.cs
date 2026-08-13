using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Padel.Application.Dashboard;

namespace Padel.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/dashboard")]
public sealed class DashboardController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> Get(CancellationToken cancellationToken)
    {
        var summary = await sender.Send(new GetDashboardSummaryQuery(), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<DashboardAnalyticsDto>> GetAnalytics(
        [FromQuery] string range, CancellationToken cancellationToken)
    {
        var analytics = await sender.Send(new GetDashboardAnalyticsQuery(range), cancellationToken);
        return Ok(analytics);
    }
}
