using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Bookings.Services;
using Padel.Application.Common;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Enums;

namespace Padel.Application.Bookings.GetAvailability;

public sealed class GetAvailabilityQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAvailabilityQuery, List<AvailabilitySlotDto>>
{
    public async Task<List<AvailabilitySlotDto>> Handle(GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var activeCourts = await context.Courts
            .Include(c => c.Schedules)
            .Where(c => c.Status == CourtStatus.Active)
            .ToListAsync(cancellationToken);

        var closuresForDate = await context.CourtClosures
            .Where(c => c.ClosureDate == request.Date)
            .ToListAsync(cancellationToken);

        // A Pending booking (Online, awaiting Thawani payment) only counts as occupying its slot
        // for a grace window — otherwise an abandoned checkout would block the slot forever, since
        // nothing else ever moves a Pending booking off it. Compared in UTC since Booking.CreatedAt
        // is stored as DateTime.UtcNow (unlike the Oman-local wall-clock checks below).
        var graceThreshold = DateTime.UtcNow.AddMinutes(-BookingPolicy.PendingPaymentGraceMinutes);

        var occupiedSlots = await context.BookingItems
            .Where(i => i.BookingDate == request.Date && i.CancelledAt == null
                && (i.Booking!.Status == BookingStatus.Confirmed
                    || (i.Booking.Status == BookingStatus.Pending && i.Booking.CreatedAt > graceThreshold)))
            .Select(i => new { i.CourtId, i.StartTime })
            .ToListAsync(cancellationToken);

        var occupied = occupiedSlots.Select(o => (o.CourtId, o.StartTime)).ToHashSet();

        var dayOfWeek = (int)request.Date.DayOfWeek;
        var now = OmanClock.Now();
        var isToday = request.Date == DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        var slots = new List<AvailabilitySlotDto>();

        for (var hour = 0; hour < 23; hour++)
        {
            var startTime = new TimeOnly(hour, 0);
            var endTime = new TimeOnly(hour + 1, 0);

            var eligibleCourts = SlotAvailabilityCalculator.GetEligibleCourts(
                activeCourts, closuresForDate, dayOfWeek, startTime, endTime);

            if (eligibleCourts.Count == 0)
            {
                continue;
            }

            var hasAvailableCourt = eligibleCourts.Any(c => !occupied.Contains((c.Id, startTime)));
            var isPast = request.Date < DateOnly.FromDateTime(now) || (isToday && startTime <= nowTime);

            slots.Add(new AvailabilitySlotDto(
                startTime,
                endTime,
                hasAvailableCourt && !isPast,
                eligibleCourts.Min(c => c.HourPrice)));
        }

        return slots;
    }
}
