using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Exceptions;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Bookings.GetBookingByReference;

public sealed class GetBookingByReferenceQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBookingByReferenceQuery, BookingStatusDto>
{
    public async Task<BookingStatusDto> Handle(GetBookingByReferenceQuery request, CancellationToken cancellationToken)
    {
        var booking = await context.Bookings
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.BookingReference == request.Reference, cancellationToken)
            ?? throw new NotFoundException(nameof(Booking), request.Reference);

        return new BookingStatusDto(
            booking.BookingReference,
            booking.Status.ToString(),
            booking.PaymentMethod.ToString(),
            booking.PaymentStatus.ToString(),
            booking.Total,
            booking.Items
                .Select(i => new BookingSlotStatusDto(i.BookingDate, i.StartTime, i.EndTime))
                .ToList());
    }
}
