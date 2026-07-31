using FluentAssertions;
using Padel.Application.Common;
using Padel.Application.Dashboard;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Dashboard;

public sealed class GetDashboardSummaryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ComputesTodaysBookingsRevenueAndOccupancy_ForConfirmedBookingsOnly()
    {
        await using var context = TestDbContextFactory.Create();
        var today = DateOnly.FromDateTime(OmanClock.Now());

        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);
        court.ReplaceSchedules([new CourtSchedule(court.Id, (int)today.DayOfWeek, new TimeOnly(8, 0), new TimeOnly(23, 0))]);
        await context.SaveChangesAsync(CancellationToken.None);

        var customer = new Customer("+96891234567", null, null);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);

        var confirmed = new Booking("PDL-TODAY1", customer.Id, PaymentMethod.PayOnArrival);
        context.Bookings.Add(confirmed);
        await context.SaveChangesAsync(CancellationToken.None);
        confirmed.AddItem(court.Id, today, new TimeOnly(9, 0), new TimeOnly(10, 0), 15m);
        confirmed.ApplyPricing(15m, 0m);
        confirmed.Confirm();

        var pending = new Booking("PDL-TODAY2", customer.Id, PaymentMethod.Online);
        context.Bookings.Add(pending);
        await context.SaveChangesAsync(CancellationToken.None);
        pending.AddItem(court.Id, today, new TimeOnly(10, 0), new TimeOnly(11, 0), 15m);
        pending.ApplyPricing(15m, 0m);

        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetDashboardSummaryQueryHandler(context);

        var summary = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // 08:00..22:00 start times = 15 open slots for the single court; only the Confirmed
        // booking counts as occupied, the Pending (Online, no grace-window relevance here since
        // it's fresh) one is excluded from this dashboard's Confirmed-only revenue/count.
        summary.TodayBookingsCount.Should().Be(1);
        summary.TodayRevenue.Should().Be(15m);
        summary.OccupancyRate.Should().BeApproximately(1.0 / 15.0, 0.0001);
    }
}
