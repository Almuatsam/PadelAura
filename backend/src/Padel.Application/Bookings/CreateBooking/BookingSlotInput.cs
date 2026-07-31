namespace Padel.Application.Bookings.CreateBooking;

public sealed record BookingSlotInput(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);
