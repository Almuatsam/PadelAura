namespace Padel.Application.Bookings.GetBookingByReference;

public sealed record BookingSlotStatusDto(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);
