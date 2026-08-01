using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Exceptions;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Courts.Commands.UpdateCourt;

public sealed class UpdateCourtCommandHandler(IApplicationDbContext context, ICurrentAdminService currentAdmin)
    : IRequestHandler<UpdateCourtCommand, CourtDto>
{
    public async Task<CourtDto> Handle(UpdateCourtCommand request, CancellationToken cancellationToken)
    {
        var court = await context.Courts
            .Include(c => c.Schedules)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Court), request.Id);

        court.Update(request.Name, request.HourPrice, request.Status);

        court.ReplaceSchedules(request.Schedules.Select(s =>
            new CourtSchedule(court.Id, s.DayOfWeek, s.OpenTime, s.CloseTime)));

        context.AuditLogs.Add(new AuditLog(
            currentAdmin.AdminId, "Update", nameof(Court), court.Id,
            JsonSerializer.Serialize(new { court.Name, court.HourPrice, Status = court.Status.ToString() })));

        await context.SaveChangesAsync(cancellationToken);

        return new CourtDto(
            court.Id,
            court.Name,
            court.HourPrice,
            court.Status.ToString(),
            court.Schedules.Select(s => new CourtScheduleDto(s.DayOfWeek, s.OpenTime, s.CloseTime)).ToList());
    }
}
