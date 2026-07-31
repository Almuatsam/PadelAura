using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common;
using Padel.Application.Common.Interfaces;

namespace Padel.Application.Closures;

public sealed class GetClosuresQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetClosuresQuery, List<ClosureDto>>
{
    public async Task<List<ClosureDto>> Handle(GetClosuresQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(OmanClock.Now());

        var closures = await context.CourtClosures
            .Include(c => c.Court)
            .Where(c => c.ClosureDate >= today)
            .OrderBy(c => c.ClosureDate)
            .ThenBy(c => c.StartTime)
            .ToListAsync(cancellationToken);

        return closures
            .Select(c => new ClosureDto(c.Id, c.CourtId, c.Court?.Name, c.ClosureDate, c.StartTime, c.EndTime, c.Reason))
            .ToList();
    }
}
