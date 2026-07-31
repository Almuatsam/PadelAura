using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Bookings.Services;
using Padel.Application.Common;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Enums;

namespace Padel.Application.Dashboard;

public sealed class GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(OmanClock.Now());

        var activeCourts = await context.Courts
            .Include(c => c.Schedules)
            .Where(c => c.Status == CourtStatus.Active)
            .ToListAsync(cancellationToken);

        var closuresForToday = await context.CourtClosures
            .Where(c => c.ClosureDate == today)
            .ToListAsync(cancellationToken);

        var dayOfWeek = (int)today.DayOfWeek;
        var totalOpenSlots = 0;

        for (var hour = 0; hour < 23; hour++)
        {
            var startTime = new TimeOnly(hour, 0);
            var endTime = new TimeOnly(hour + 1, 0);

            totalOpenSlots += SlotAvailabilityCalculator
                .GetEligibleCourts(activeCourts, closuresForToday, dayOfWeek, startTime, endTime)
                .Count;
        }

        var todayItems = await context.BookingItems
            .Include(i => i.Booking)
            .Where(i => i.BookingDate == today && i.CancelledAt == null && i.Booking!.Status == BookingStatus.Confirmed)
            .ToListAsync(cancellationToken);

        var todayBookingsCount = todayItems.Select(i => i.BookingId).Distinct().Count();
        var todayRevenue = todayItems.Sum(i => i.Price);
        var occupancyRate = totalOpenSlots == 0 ? 0 : (double)todayItems.Count / totalOpenSlots;

        return new DashboardSummaryDto(todayBookingsCount, todayRevenue, occupancyRate);
    }
}
