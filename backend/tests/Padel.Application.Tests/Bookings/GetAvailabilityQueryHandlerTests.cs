using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Bookings;
using Padel.Application.Bookings.GetAvailability;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Bookings;

public sealed class GetAvailabilityQueryHandlerTests
{
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

    [Fact]
    public async Task Handle_ReturnsAvailableSlots_MatchingCourtSchedule()
    {
        await using var context = TestDbContextFactory.Create();
        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);

        court.ReplaceSchedules([new CourtSchedule(court.Id, (int)FutureDate.DayOfWeek, new TimeOnly(8, 0), new TimeOnly(23, 0))]);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAvailabilityQueryHandler(context);

        var slots = await handler.Handle(new GetAvailabilityQuery(FutureDate), CancellationToken.None);

        slots.Should().HaveCount(15); // 08:00..22:00 start times
        slots.Should().OnlyContain(s => s.IsAvailable && s.Price == 15m);
    }

    [Fact]
    public async Task Handle_MarksSlotUnavailable_WhenTheOnlyEligibleCourtIsBooked()
    {
        await using var context = TestDbContextFactory.Create();
        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);

        court.ReplaceSchedules([new CourtSchedule(court.Id, (int)FutureDate.DayOfWeek, new TimeOnly(8, 0), new TimeOnly(23, 0))]);

        var booking = new Booking("PDL-TEST01", 1, Padel.Domain.Enums.PaymentMethod.PayOnArrival);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync(CancellationToken.None);

        booking.AddItem(court.Id, FutureDate, new TimeOnly(9, 0), new TimeOnly(10, 0), 15m);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAvailabilityQueryHandler(context);

        var slots = await handler.Handle(new GetAvailabilityQuery(FutureDate), CancellationToken.None);

        var nineAm = slots.Single(s => s.StartTime == new TimeOnly(9, 0));
        nineAm.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_IgnoresAbandonedPendingOnlineBooking_PastTheGraceWindow()
    {
        await using var context = TestDbContextFactory.Create();
        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);

        court.ReplaceSchedules([new CourtSchedule(court.Id, (int)FutureDate.DayOfWeek, new TimeOnly(8, 0), new TimeOnly(23, 0))]);

        var customer = new Customer("+96891234567", null, null);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);

        var booking = new Booking("PDL-STALE1", customer.Id, PaymentMethod.Online);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync(CancellationToken.None);
        booking.AddItem(court.Id, FutureDate, new TimeOnly(9, 0), new TimeOnly(10, 0), 15m);
        await context.SaveChangesAsync(CancellationToken.None);

        // Backdate past the grace window — an abandoned Online checkout.
        context.Entry(booking).Property(b => b.CreatedAt).CurrentValue =
            DateTime.UtcNow.AddMinutes(-(BookingPolicy.PendingPaymentGraceMinutes + 5));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAvailabilityQueryHandler(context);

        var slots = await handler.Handle(new GetAvailabilityQuery(FutureDate), CancellationToken.None);

        var nineAm = slots.Single(s => s.StartTime == new TimeOnly(9, 0));
        nineAm.IsAvailable.Should().BeTrue();
    }
}
