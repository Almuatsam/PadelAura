namespace Padel.Application.Bookings.GetAdminBookings;

public sealed record AdminBookingItemDto(
    string CourtName,
    DateOnly BookingDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal Price);

// Unlike the customer-facing BookingStatusDto, admins are allowed to see which court was assigned.
public sealed record AdminBookingListItemDto(
    long Id,
    string BookingReference,
    string CustomerPhone,
    string? CustomerName,
    string Status,
    string PaymentMethod,
    string PaymentStatus,
    decimal Total,
    DateTime CreatedAt,
    IReadOnlyList<AdminBookingItemDto> Items);
