using MediatR;

namespace Padel.Application.Courts.Commands.DeleteCourt;

public sealed record DeleteCourtCommand(long Id) : IRequest;
