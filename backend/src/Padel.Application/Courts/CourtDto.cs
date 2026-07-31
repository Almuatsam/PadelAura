namespace Padel.Application.Courts;

public sealed record CourtDto(
    long Id,
    string Name,
    decimal HourPrice,
    string Status,
    IReadOnlyList<CourtScheduleDto> Schedules);
