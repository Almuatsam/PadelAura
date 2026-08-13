using FluentAssertions;
using Padel.Application.Common;
using Padel.Application.Dashboard;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Dashboard;

public sealed class GetDashboardAnalyticsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsSevenDayOccupancyAndRevenue_ForWeekRange()
    {
        await using var context = TestDbContextFactory.Create();
        var today = DateOnly.FromDateTime(OmanClock.Now());
        var startDate = today.AddDays(-6);

        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);
        court.ReplaceSchedules(Enumerable.Range(0, 7)
            .Select(dayOfWeek => new CourtSchedule(court.Id, dayOfWeek, new TimeOnly(8, 0), new TimeOnly(23, 0))));
        await context.SaveChangesAsync(CancellationToken.None);

        var customer = new Customer("+96891234567", null, null);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);

        // Item dated at the start of the range, but created "now" — illustrates the deliberate
        // axis split: occupancy buckets by BookingDate (slot was for), revenue by CreatedAt
        // (money was made on).
        var confirmed = new Booking("PDL-WEEK01", customer.Id, PaymentMethod.PayOnArrival);
        context.Bookings.Add(confirmed);
        await context.SaveChangesAsync(CancellationToken.None);
        confirmed.AddItem(court.Id, startDate, new TimeOnly(9, 0), new TimeOnly(10, 0), 15m);
        confirmed.ApplyPricing(15m, 0m);
        confirmed.Confirm();

        var completed = new Booking("PDL-WEEK02", customer.Id, PaymentMethod.PayOnArrival);
        context.Bookings.Add(completed);
        await context.SaveChangesAsync(CancellationToken.None);
        completed.AddItem(court.Id, today, new TimeOnly(14, 0), new TimeOnly(15, 0), 20m);
        completed.ApplyPricing(20m, 0m);
        completed.Confirm();
        completed.Complete();

        var cancelled = new Booking("PDL-WEEK03", customer.Id, PaymentMethod.PayOnArrival);
        context.Bookings.Add(cancelled);
        await context.SaveChangesAsync(CancellationToken.None);
        cancelled.AddItem(court.Id, today, new TimeOnly(16, 0), new TimeOnly(17, 0), 15m);
        cancelled.ApplyPricing(15m, 0m);
        cancelled.Confirm();
        cancelled.Cancel();

        var pending = new Booking("PDL-WEEK04", customer.Id, PaymentMethod.Online);
        context.Bookings.Add(pending);
        await context.SaveChangesAsync(CancellationToken.None);
        pending.AddItem(court.Id, today, new TimeOnly(18, 0), new TimeOnly(19, 0), 15m);
        pending.ApplyPricing(15m, 0m);

        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDashboardAnalyticsQueryHandler(context);

        var result = await handler.Handle(new GetDashboardAnalyticsQuery("week"), CancellationToken.None);

        result.Occupancy.Should().HaveCount(7);
        result.Occupancy.First().Date.Should().Be(startDate);
        result.Occupancy.Last().Date.Should().Be(today);
        result.Occupancy.Single(o => o.Date == startDate).BookingsCount.Should().Be(1);
        result.Occupancy.Single(o => o.Date == today).BookingsCount.Should().Be(1);

        result.Revenue.Should().HaveCount(7);
        // Both counted bookings were created "now" (real UtcNow), which is "today" in Oman terms.
        result.Revenue.Single(r => r.Date == today).Revenue.Should().Be(35m);
        result.TotalRevenue.Should().Be(35m);

        // 08:00..22:00 start times = 15 open slots/day for the single court, across 7 days.
        result.OccupancyRatePercent.Should().BeApproximately(2.0 / 105.0 * 100, 0.01);
    }

    [Fact]
    public async Task Handle_ReturnsThirtyDayRange_ForMonthRange()
    {
        await using var context = TestDbContextFactory.Create();
        var today = DateOnly.FromDateTime(OmanClock.Now());

        var handler = new GetDashboardAnalyticsQueryHandler(context);

        var result = await handler.Handle(new GetDashboardAnalyticsQuery("month"), CancellationToken.None);

        result.Occupancy.Should().HaveCount(30);
        result.Occupancy.First().Date.Should().Be(today.AddDays(-29));
        result.Occupancy.Last().Date.Should().Be(today);
        result.Revenue.Should().HaveCount(30);
        result.TotalRevenue.Should().Be(0m);
        // No active courts seeded — denominator is 0, guarded to avoid a divide-by-zero.
        result.OccupancyRatePercent.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ComputesTop5BusiestHours_OrderedByCountDescending()
    {
        await using var context = TestDbContextFactory.Create();
        var today = DateOnly.FromDateTime(OmanClock.Now());
        var startDate = today.AddDays(-6);

        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        var customer = new Customer("+96891234567", null, null);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);

        // hour -> distinct-date offsets it appears on (offset is unique per hour so no two items
        // ever share the same court+date+start-time).
        var hourToOffsets = new Dictionary<int, int[]>
        {
            [9] = [0, 1, 2, 3, 4],
            [10] = [0, 1, 2, 3],
            [11] = [0, 1, 2],
            [12] = [0, 1],
            [13] = [0],
            [14] = [1],
        };

        var sequence = 0;
        foreach (var (hour, offsets) in hourToOffsets)
        {
            foreach (var offset in offsets)
            {
                var booking = new Booking($"PDL-HOUR{sequence++:D2}", customer.Id, PaymentMethod.PayOnArrival);
                context.Bookings.Add(booking);
                await context.SaveChangesAsync(CancellationToken.None);
                booking.AddItem(court.Id, startDate.AddDays(offset), new TimeOnly(hour, 0), new TimeOnly(hour + 1, 0), 15m);
                booking.ApplyPricing(15m, 0m);
                booking.Confirm();
                await context.SaveChangesAsync(CancellationToken.None);
            }
        }

        var handler = new GetDashboardAnalyticsQueryHandler(context);

        var result = await handler.Handle(new GetDashboardAnalyticsQuery("week"), CancellationToken.None);

        result.BusiestHours.Should().HaveCount(5);
        result.BusiestHours.Select(h => h.Hour).Should().Equal(9, 10, 11, 12, 13);
        result.BusiestHours.Select(h => h.Count).Should().Equal(5, 4, 3, 2, 1);
    }

    [Fact]
    public async Task Handle_IncludesCompletedBookings_NotJustConfirmed()
    {
        await using var context = TestDbContextFactory.Create();
        var today = DateOnly.FromDateTime(OmanClock.Now());

        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        var customer = new Customer("+96891234567", null, null);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);

        var completed = new Booking("PDL-COMP01", customer.Id, PaymentMethod.PayOnArrival);
        context.Bookings.Add(completed);
        await context.SaveChangesAsync(CancellationToken.None);
        completed.AddItem(court.Id, today, new TimeOnly(9, 0), new TimeOnly(10, 0), 15m);
        completed.ApplyPricing(15m, 0m);
        completed.Confirm();
        completed.Complete();
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDashboardAnalyticsQueryHandler(context);

        var result = await handler.Handle(new GetDashboardAnalyticsQuery("week"), CancellationToken.None);

        result.Occupancy.Single(o => o.Date == today).BookingsCount.Should().Be(1);
        result.TotalRevenue.Should().Be(15m);
    }

    [Fact]
    public async Task Handle_ExcludesCancelledBookingItems()
    {
        await using var context = TestDbContextFactory.Create();
        var today = DateOnly.FromDateTime(OmanClock.Now());

        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        var customer = new Customer("+96891234567", null, null);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);

        var cancelled = new Booking("PDL-CANC01", customer.Id, PaymentMethod.PayOnArrival);
        context.Bookings.Add(cancelled);
        await context.SaveChangesAsync(CancellationToken.None);
        cancelled.AddItem(court.Id, today, new TimeOnly(9, 0), new TimeOnly(10, 0), 15m);
        cancelled.ApplyPricing(15m, 0m);
        cancelled.Confirm();
        cancelled.Cancel();
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDashboardAnalyticsQueryHandler(context);

        var result = await handler.Handle(new GetDashboardAnalyticsQuery("week"), CancellationToken.None);

        result.Occupancy.Should().OnlyContain(o => o.BookingsCount == 0);
        result.Revenue.Should().OnlyContain(r => r.Revenue == 0m);
        result.BusiestHours.Should().BeEmpty();
    }
}
