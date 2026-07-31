namespace Padel.Application.Courts;

public sealed record CourtScheduleInput(int DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime);
