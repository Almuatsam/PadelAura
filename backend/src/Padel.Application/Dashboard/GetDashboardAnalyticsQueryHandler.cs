using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Bookings.Services;
using Padel.Application.Common;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Enums;

namespace Padel.Application.Dashboard;

public sealed class GetDashboardAnalyticsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDashboardAnalyticsQuery, DashboardAnalyticsDto>
{
    private const int BusiestHoursLimit = 5;

    public async Task<DashboardAnalyticsDto> Handle(GetDashboardAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // Fixed day-counts (last 7 / last 30 calendar days including today, Oman local) rather
        // than "this ISO week/month" — deterministic, trivially testable, no variable-length-
        // month edge cases.
        var endDate = DateOnly.FromDateTime(OmanClock.Now());
        var days = request.Range == "week" ? 7 : 30;
        var startDate = endDate.AddDays(-(days - 1));

        // Confirmed + Completed (unlike GetDashboardSummaryQueryHandler's Confirmed-only "today"
        // filter) — matches Feature-Additions.md's revenue query and is applied uniformly across
        // occupancy/revenue/busiest-hours so the three views of this endpoint agree with each
        // other. Fetch-then-aggregate in memory, mirroring GetDashboardSummaryQueryHandler's
        // style: this codebase has no GroupBy-in-SQL precedent, no HTTP integration test project
        // to catch a MySQL-translation mismatch against the EF Core InMemory tests, and the data
        // volume here (a demo project) makes the in-memory cost irrelevant. Only simple,
        // reliably-translatable range/status WHERE predicates hit the DB. At real production
        // volume this would need to move to DB-side aggregation.
        var items = await context.BookingItems
            .Include(i => i.Booking)
            .Where(i => i.BookingDate >= startDate && i.BookingDate <= endDate
                && i.CancelledAt == null
                && (i.Booking!.Status == BookingStatus.Confirmed || i.Booking!.Status == BookingStatus.Completed))
            .ToListAsync(cancellationToken);

        var occupancy = Enumerable.Range(0, days)
            .Select(offset => startDate.AddDays(offset))
            .Select(date => new DailyOccupancyDto(date, items.Count(i => i.BookingDate == date)))
            .ToList();

        var busiestHours = items
            .GroupBy(i => i.StartTime.Hour)
            .Select(g => new BusiestHourDto(g.Key, g.Count()))
            .OrderByDescending(h => h.Count)
            .ThenBy(h => h.Hour)
            .Take(BusiestHoursLimit)
            .ToList();

        // Revenue buckets by Booking.CreatedAt (UTC, "when the money was made"), a deliberately
        // different axis from occupancy's BookingDate ("what the slot was for") — matches
        // Feature-Additions.md's own SQL sketch.
        var rangeStartUtc = OmanClock.StartOfOmanDateUtc(startDate);
        var rangeEndUtc = OmanClock.StartOfOmanDateUtc(endDate.AddDays(1));

        var bookingsInRange = await context.Bookings
            .Where(b => b.CreatedAt >= rangeStartUtc && b.CreatedAt < rangeEndUtc
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed))
            .ToListAsync(cancellationToken);

        var revenue = Enumerable.Range(0, days)
            .Select(offset => startDate.AddDays(offset))
            .Select(date => new DailyRevenueDto(
                date,
                bookingsInRange.Where(b => OmanClock.ToOmanDate(b.CreatedAt) == date).Sum(b => b.Total)))
            .ToList();

        var totalRevenue = revenue.Sum(r => r.Revenue);

        var totalOpenSlotsInRange = await GetTotalOpenSlotsAsync(startDate, endDate, cancellationToken);
        var occupancyRatePercent = totalOpenSlotsInRange == 0
            ? 0
            : (double)items.Count / totalOpenSlotsInRange * 100;

        return new DashboardAnalyticsDto(occupancy, revenue, busiestHours, totalRevenue, occupancyRatePercent);
    }

    private async Task<int> GetTotalOpenSlotsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var activeCourts = await context.Courts
            .Include(c => c.Schedules)
            .Where(c => c.Status == CourtStatus.Active)
            .ToListAsync(cancellationToken);

        var closuresInRange = await context.CourtClosures
            .Where(c => c.ClosureDate >= startDate && c.ClosureDate <= endDate)
            .ToListAsync(cancellationToken);

        var totalOpenSlots = 0;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var closuresForDate = closuresInRange.Where(c => c.ClosureDate == date).ToList();
            var dayOfWeek = (int)date.DayOfWeek;

            for (var hour = 0; hour < 23; hour++)
            {
                var startTime = new TimeOnly(hour, 0);
                var endTime = new TimeOnly(hour + 1, 0);

                totalOpenSlots += SlotAvailabilityCalculator
                    .GetEligibleCourts(activeCourts, closuresForDate, dayOfWeek, startTime, endTime)
                    .Count;
            }
        }

        return totalOpenSlots;
    }
}
