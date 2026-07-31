namespace Padel.Application.Bookings.GetBookingByReference;

// Deliberately excludes court id/name: customers never see which court they were assigned.
public sealed record BookingStatusDto(
    string BookingReference,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    decimal Total,
    IReadOnlyList<BookingSlotStatusDto> Slots);
