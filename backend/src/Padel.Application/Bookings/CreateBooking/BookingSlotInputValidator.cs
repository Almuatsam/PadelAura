using FluentValidation;
using Padel.Application.Common;

namespace Padel.Application.Bookings.CreateBooking;

public sealed class BookingSlotInputValidator : AbstractValidator<BookingSlotInput>
{
    public BookingSlotInputValidator()
    {
        RuleFor(x => x.StartTime)
            .Must(t => t.Minute == 0 && t.Second == 0)
            .WithMessage("startTime must be aligned to the hour.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage("endTime must be after startTime.");

        RuleFor(x => x)
            .Must(x => x.EndTime == x.StartTime.AddHours(1))
            .WithMessage("Each slot must be exactly one hour long.")
            .WithName("EndTime");

        RuleFor(x => x)
            .Must(NotBeInThePast)
            .WithMessage("A slot cannot be in the past.")
            .WithName("Date");
    }

    private static bool NotBeInThePast(BookingSlotInput slot)
    {
        var now = OmanClock.Now();
        var today = DateOnly.FromDateTime(now);

        if (slot.Date < today)
        {
            return false;
        }

        return slot.Date != today || slot.StartTime > TimeOnly.FromDateTime(now);
    }
}
