using FluentAssertions;
using Padel.Application.Bookings.GetAdminBookings;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Bookings;

public sealed class GetAdminBookingsQueryHandlerTests
{
    private static readonly DateOnly SlotDate = new(2026, 8, 1);

    private static async Task<Court> SeedBookingsAsync(Padel.Infrastructure.Persistence.PadelDbContext context)
    {
        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);

        var customerA = new Customer("+96891111111", "Ali", null);
        var customerB = new Customer("+96892222222", "Sara", null);
        context.Customers.AddRange(customerA, customerB);
        await context.SaveChangesAsync(CancellationToken.None);

        var confirmed = new Booking("PDL-CONFIRM", customerA.Id, PaymentMethod.PayOnArrival);
        context.Bookings.Add(confirmed);
        await context.SaveChangesAsync(CancellationToken.None);
        confirmed.AddItem(court.Id, SlotDate, new TimeOnly(9, 0), new TimeOnly(10, 0), 15m);
        confirmed.ApplyPricing(15m, 0m);
        confirmed.Confirm();

        var pending = new Booking("PDL-PENDING", customerB.Id, PaymentMethod.Online);
        context.Bookings.Add(pending);
        await context.SaveChangesAsync(CancellationToken.None);
        pending.AddItem(court.Id, SlotDate, new TimeOnly(11, 0), new TimeOnly(12, 0), 15m);
        pending.ApplyPricing(15m, 0m);

        await context.SaveChangesAsync(CancellationToken.None);

        return court;
    }

    [Fact]
    public async Task Handle_ReturnsBookingsWithCourtNameExposed_MostRecentFirst()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedBookingsAsync(context);

        var handler = new GetAdminBookingsQueryHandler(context);
        var result = await handler.Handle(
            new GetAdminBookingsQuery(null, null, null, null, null),
            CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.Items.All(i => i.CourtName == "Court A"));
    }

    [Fact]
    public async Task Handle_FiltersByStatusAndPhone()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedBookingsAsync(context);

        var handler = new GetAdminBookingsQueryHandler(context);

        var byStatus = await handler.Handle(
            new GetAdminBookingsQuery(null, null, BookingStatus.Confirmed, null, null),
            CancellationToken.None);
        byStatus.Should().ContainSingle(b => b.BookingReference == "PDL-CONFIRM");

        var byPhone = await handler.Handle(
            new GetAdminBookingsQuery(null, null, null, null, "92222222"),
            CancellationToken.None);
        byPhone.Should().ContainSingle(b => b.BookingReference == "PDL-PENDING");
    }
}
