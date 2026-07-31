namespace Padel.Application.Common.Exceptions;

public sealed class SlotUnavailableException(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    : Exception($"The slot {startTime}-{endTime} on {date} is no longer available.");
