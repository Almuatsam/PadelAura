namespace Padel.Application.Bookings;

public sealed record AvailabilitySlotDto(TimeOnly StartTime, TimeOnly EndTime, bool IsAvailable, decimal Price);
