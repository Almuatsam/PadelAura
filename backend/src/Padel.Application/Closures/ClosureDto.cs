namespace Padel.Application.Closures;

public sealed record ClosureDto(
    long Id,
    long? CourtId,
    string? CourtName,
    DateOnly ClosureDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason);
