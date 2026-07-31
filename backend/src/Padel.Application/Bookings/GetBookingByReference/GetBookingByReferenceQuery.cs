using MediatR;

namespace Padel.Application.Bookings.GetBookingByReference;

public sealed record GetBookingByReferenceQuery(string Reference) : IRequest<BookingStatusDto>;
