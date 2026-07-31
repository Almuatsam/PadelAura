using MediatR;

namespace Padel.Application.Courts.Queries.GetCourts;

public sealed record GetCourtsQuery : IRequest<List<CourtDto>>;
