namespace Padel.Application.Courts;

public sealed record CourtScheduleDto(int DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime);
