using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Interfaces;

namespace Padel.Application.Courts.Queries.GetCourts;

public sealed class GetCourtsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCourtsQuery, List<CourtDto>>
{
    public async Task<List<CourtDto>> Handle(GetCourtsQuery request, CancellationToken cancellationToken)
    {
        var courts = await context.Courts
            .Include(c => c.Schedules)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

        return courts
            .Select(court => new CourtDto(
                court.Id,
                court.Name,
                court.HourPrice,
                court.Status.ToString(),
                court.Schedules
                    .Select(s => new CourtScheduleDto(s.DayOfWeek, s.OpenTime, s.CloseTime))
                    .ToList()))
            .ToList();
    }
}
