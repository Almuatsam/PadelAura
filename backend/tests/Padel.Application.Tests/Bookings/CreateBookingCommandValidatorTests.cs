using FluentAssertions;
using Padel.Application.Bookings;
using Padel.Application.Bookings.CreateBooking;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Bookings;

public sealed class CreateBookingCommandValidatorTests
{
    private readonly CreateBookingCommandValidator _validator = new();

    private static List<BookingSlotInput> BuildDistinctFutureSlots(int count) =>
        Enumerable.Range(1, count)
            .Select(offset => new BookingSlotInput(
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(offset + 5)),
                new TimeOnly(9, 0),
                new TimeOnly(10, 0)))
            .ToList();

    [Fact]
    public void Validate_Succeeds_WhenSlotCountIsAtTheMax()
    {
        var command = new CreateBookingCommand(
            "+96891234567", null, null, PaymentMethod.PayOnArrival,
            BuildDistinctFutureSlots(BookingPolicy.MaxSlotsPerBooking));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenSlotCountExceedsTheMax()
    {
        var command = new CreateBookingCommand(
            "+96891234567", null, null, PaymentMethod.PayOnArrival,
            BuildDistinctFutureSlots(BookingPolicy.MaxSlotsPerBooking + 1));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
