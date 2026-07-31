using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Exceptions;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Closures;

public sealed class CreateClosureCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateClosureCommand, List<long>>
{
    public async Task<List<long>> Handle(CreateClosureCommand request, CancellationToken cancellationToken)
    {
        List<CourtClosure> closures;

        if (request.CourtIds is null || request.CourtIds.Count == 0)
        {
            closures = [new CourtClosure(null, request.Date, request.StartTime, request.EndTime, request.Reason)];
        }
        else
        {
            var existingIds = await context.Courts
                .Where(c => request.CourtIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            var missingIds = request.CourtIds.Except(existingIds).ToList();
            if (missingIds.Count != 0)
            {
                throw new NotFoundException(nameof(Court), string.Join(", ", missingIds));
            }

            closures = request.CourtIds
                .Select(courtId => new CourtClosure(courtId, request.Date, request.StartTime, request.EndTime, request.Reason))
                .ToList();
        }

        context.CourtClosures.AddRange(closures);

        await context.SaveChangesAsync(cancellationToken);

        return closures.Select(c => c.Id).ToList();
    }
}
