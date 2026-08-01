using FluentValidation;
using Padel.Application.Bookings.CreateBooking;

namespace Padel.Application.Bookings.GetBookingQuote;

public sealed class GetBookingQuoteQueryValidator : AbstractValidator<GetBookingQuoteQuery>
{
    public GetBookingQuoteQueryValidator()
    {
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
