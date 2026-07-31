using Padel.Application.Courts;
using Padel.Domain.Enums;

namespace Padel.Api.Controllers.Admin;

public sealed record UpdateCourtRequest(
    string Name,
    decimal HourPrice,
    CourtStatus Status,
    IReadOnlyList<CourtScheduleInput> Schedules);
