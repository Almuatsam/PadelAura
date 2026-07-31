using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Padel.Application.Bookings.CreateBooking;
using Padel.Application.Bookings.GetBookingByReference;

namespace Padel.Api.Controllers.Customer;

[ApiController]
[Route("api/customer")]
public sealed class BookingsController(ISender sender) : ControllerBase
{
    [HttpPost("book")]
    [EnableRateLimiting("booking")]
    public async Task<ActionResult<CreateBookingResult>> Book(
        CreateBookingCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("bookings/{reference}")]
    [EnableRateLimiting("lookup")]
    public async Task<ActionResult<BookingStatusDto>> GetByReference(
        string reference, CancellationToken cancellationToken)
    {
        var booking = await sender.Send(new GetBookingByReferenceQuery(reference), cancellationToken);
        return Ok(booking);
    }
}
