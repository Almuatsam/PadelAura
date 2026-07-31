using FluentAssertions;
using Padel.Application.Bookings.GetAvailability;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;

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
}
