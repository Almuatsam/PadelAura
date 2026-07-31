using MediatR;

namespace Padel.Application.Closures;

public sealed record CreateClosureCommand(
    IReadOnlyList<long>? CourtIds,
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason) : IRequest<List<long>>;
