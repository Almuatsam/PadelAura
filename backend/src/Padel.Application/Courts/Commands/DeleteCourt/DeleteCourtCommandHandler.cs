using MediatR;
using Padel.Application.Common.Exceptions;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Courts.Commands.DeleteCourt;

public sealed class DeleteCourtCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteCourtCommand>
{
    public async Task Handle(DeleteCourtCommand request, CancellationToken cancellationToken)
    {
        var court = await context.Courts.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException(nameof(Court), request.Id);

        context.Courts.Remove(court);

        await context.SaveChangesAsync(cancellationToken);
    }
}
