using MediatR;

namespace Padel.Application.Closures;

public sealed record GetClosuresQuery : IRequest<List<ClosureDto>>;
