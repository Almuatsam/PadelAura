using FluentAssertions;
using Padel.Application.Bookings.CreateBooking;
using Padel.Application.Common.Exceptions;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Bookings;

public sealed class CreateBookingCommandHandlerTests
{
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

    private static async Task<Court> SeedCourtAsync(Padel.Infrastructure.Persistence.PadelDbContext context, decimal hourPrice = 15m)
    {
        var court = new Court("Court A", hourPrice);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);

        court.ReplaceSchedules([new CourtSchedule(court.Id, (int)FutureDate.DayOfWeek, new TimeOnly(8, 0), new TimeOnly(23, 0))]);
        await context.SaveChangesAsync(CancellationToken.None);

        return court;
    }

    [Fact]
    public async Task Handle_CreatesConfirmedBooking_ForPayOnArrival()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedCourtAsync(context);

        var handler = new CreateBookingCommandHandler(new TestApplicationDbContext(context));
        var command = new CreateBookingCommand(
            "+96891234567", "Ali", null, PaymentMethod.PayOnArrival,
            [new BookingSlotInput(FutureDate, new TimeOnly(9, 0), new TimeOnly(10, 0))]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.BookingReference.Should().StartWith("PDL-");
        result.Total.Should().Be(15m);

        var booking = context.Bookings.Single();
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.Items.Should().ContainSingle(i => i.CourtId == context.Courts.Single().Id);
        context.Customers.Should().ContainSingle(c => c.Phone == "+96891234567");
    }

    [Fact]
    public async Task Handle_ReusesExistingCustomer_ByPhone()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedCourtAsync(context);
        var existingCustomer = new Customer("+96891234567", "Existing", null);
        context.Customers.Add(existingCustomer);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateBookingCommandHandler(new TestApplicationDbContext(context));
        var command = new CreateBookingCommand(
            "+96891234567", null, null, PaymentMethod.PayOnArrival,
            [new BookingSlotInput(FutureDate, new TimeOnly(9, 0), new TimeOnly(10, 0))]);

        await handler.Handle(command, CancellationToken.None);

        context.Customers.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ThrowsSlotUnavailableException_WhenNoCourtIsScheduledForThatWindow()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedCourtAsync(context);

        var handler = new CreateBookingCommandHandler(new TestApplicationDbContext(context));
        var command = new CreateBookingCommand(
            "+96891234567", null, null, PaymentMethod.PayOnArrival,
            [new BookingSlotInput(FutureDate, new TimeOnly(6, 0), new TimeOnly(7, 0))]);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<SlotUnavailableException>();
    }

    [Fact]
    public async Task Handle_PersistsNothing_WhenAnySlotInTheCartHasNoAvailableCourt()
    {
        await using var context = TestDbContextFactory.Create();
        var court = await SeedCourtAsync(context);

        // Pre-book the only court for the second requested slot.
        var existingCustomer = new Customer("+96899999999", null, null);
        context.Customers.Add(existingCustomer);
        await context.SaveChangesAsync(CancellationToken.None);
        var existingBooking = new Booking("PDL-EXIST1", existingCustomer.Id, PaymentMethod.PayOnArrival);
        context.Bookings.Add(existingBooking);
        await context.SaveChangesAsync(CancellationToken.None);
        existingBooking.AddItem(court.Id, FutureDate, new TimeOnly(11, 0), new TimeOnly(12, 0), 15m);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateBookingCommandHandler(new TestApplicationDbContext(context));
        var command = new CreateBookingCommand(
            "+96891234567", null, null, PaymentMethod.PayOnArrival,
            [
                new BookingSlotInput(FutureDate, new TimeOnly(9, 0), new TimeOnly(10, 0)),
                new BookingSlotInput(FutureDate, new TimeOnly(11, 0), new TimeOnly(12, 0)),
            ]);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<SlotUnavailableException>();
        context.Bookings.Should().ContainSingle(b => b.BookingReference == "PDL-EXIST1");
        context.Customers.Should().NotContain(c => c.Phone == "+96891234567");
    }
}
