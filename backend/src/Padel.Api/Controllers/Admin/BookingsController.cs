using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Padel.Application.Bookings.GetAdminBookings;
using Padel.Domain.Enums;

namespace Padel.Api.Controllers.Admin;

[ApiController]
[Authorize]
[Route("api/admin/bookings")]
public sealed class BookingsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AdminBookingListItemDto>>> GetAll(
        [FromQuery] long? courtId,
        [FromQuery] DateOnly? date,
        [FromQuery] BookingStatus? status,
        [FromQuery] PaymentMethod? paymentMethod,
        [FromQuery] string? phone,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminBookingsQuery(courtId, date, status, paymentMethod, phone, page);
        var bookings = await sender.Send(query, cancellationToken);
        return Ok(bookings);
    }
}
