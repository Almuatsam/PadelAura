using MediatR;
using Padel.Application.Courts;
using Padel.Domain.Enums;

namespace Padel.Application.Courts.Commands.UpdateCourt;

public sealed record UpdateCourtCommand(
    long Id,
    string Name,
    decimal HourPrice,
    CourtStatus Status,
    IReadOnlyList<CourtScheduleInput> Schedules) : IRequest<CourtDto>;
