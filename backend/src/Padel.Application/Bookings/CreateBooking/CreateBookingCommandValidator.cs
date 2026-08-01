using FluentValidation;

namespace Padel.Application.Bookings.CreateBooking;

public sealed class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{8,15}$")
            .WithMessage("phone must be a valid phone number.");

        RuleFor(x => x.PaymentMethod).IsInEnum();

        RuleFor(x => x.Slots)
            .NotEmpty()
            .Must(slots => slots.Count <= BookingPolicy.MaxSlotsPerBooking)
            .WithMessage($"A booking cannot contain more than {BookingPolicy.MaxSlotsPerBooking} slots.");

        RuleForEach(x => x.Slots).SetValidator(new BookingSlotInputValidator());

        RuleFor(x => x.Slots)
            .Must(slots => slots.Select(s => (s.Date, s.StartTime)).Distinct().Count() == slots.Count)
            .WithMessage("The cart contains duplicate slots.")
            .When(x => x.Slots.Count > 0);
    }
}
