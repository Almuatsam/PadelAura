using System.Text.Json;
using MediatR;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Courts.Commands.CreateCourt;

public sealed class CreateCourtCommandHandler(IApplicationDbContext context, ICurrentAdminService currentAdmin)
    : IRequestHandler<CreateCourtCommand, long>
{
    public async Task<long> Handle(CreateCourtCommand request, CancellationToken cancellationToken)
    {
        var court = new Court(request.Name, request.HourPrice);

        context.Courts.Add(court);

        // Save first so `court.Id` is populated by the database before it's used as the FK
        // for the schedule rows below (mirrors DbSeeder's court/schedule insert order).
        await context.SaveChangesAsync(cancellationToken);

        court.ReplaceSchedules(request.Schedules.Select(s =>
            new CourtSchedule(court.Id, s.DayOfWeek, s.OpenTime, s.CloseTime)));

        context.AuditLogs.Add(new AuditLog(
            currentAdmin.AdminId, "Create", nameof(Court), court.Id,
            JsonSerializer.Serialize(new { court.Name, court.HourPrice })));

        await context.SaveChangesAsync(cancellationToken);

        return court.Id;
    }
}
