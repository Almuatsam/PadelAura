using MediatR;
using Padel.Application.Courts;

namespace Padel.Application.Courts.Commands.CreateCourt;

public sealed record CreateCourtCommand(
    string Name,
    decimal HourPrice,
    IReadOnlyList<CourtScheduleInput> Schedules) : IRequest<long>;
