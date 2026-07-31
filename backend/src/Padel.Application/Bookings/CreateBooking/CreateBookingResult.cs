namespace Padel.Application.Bookings.CreateBooking;

public sealed record CreateBookingResult(string BookingReference, decimal Total, string? PaymentUrl);
