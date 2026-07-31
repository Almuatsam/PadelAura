using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Padel.Application.Bookings;
using Padel.Application.Bookings.GetAvailability;

namespace Padel.Api.Controllers.Customer;

[ApiController]
[Route("api/customer/availability")]
public sealed class AvailabilityController(ISender sender) : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting("availability")]
    public async Task<ActionResult<List<AvailabilitySlotDto>>> Get(
        [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var slots = await sender.Send(new GetAvailabilityQuery(date), cancellationToken);
        return Ok(slots);
    }
}
