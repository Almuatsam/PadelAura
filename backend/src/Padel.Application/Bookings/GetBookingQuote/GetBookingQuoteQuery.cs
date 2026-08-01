using MediatR;
using Padel.Application.Bookings.CreateBooking;

namespace Padel.Application.Bookings.GetBookingQuote;

public sealed record GetBookingQuoteQuery(IReadOnlyList<BookingSlotInput> Slots) : IRequest<BookingQuoteDto>;
