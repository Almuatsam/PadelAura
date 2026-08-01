using FluentAssertions;
using Padel.Application.Common.Exceptions;
using Padel.Application.Courts;
using Padel.Application.Courts.Commands.UpdateCourt;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Courts;

public sealed class UpdateCourtCommandHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesCourtAndReplacesSchedules()
    {
        await using var context = TestDbContextFactory.Create();
        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateCourtCommandHandler(context, new FakeCurrentAdminService());
        var command = new UpdateCourtCommand(
            court.Id,
            "Court A Renamed",
            20m,
            CourtStatus.Inactive,
            [new CourtScheduleInput(1, new TimeOnly(9, 0), new TimeOnly(22, 0))]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Court A Renamed");
        result.HourPrice.Should().Be(20m);
        result.Status.Should().Be(nameof(CourtStatus.Inactive));
        result.Schedules.Should().ContainSingle(s => s.DayOfWeek == 1);

        context.AuditLogs.Should().ContainSingle(a => a.Action == "Update" && a.EntityType == "Court" && a.EntityId == court.Id);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenCourtDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new UpdateCourtCommandHandler(context, new FakeCurrentAdminService());
        var command = new UpdateCourtCommand(999, "Court X", 15m, CourtStatus.Active, []);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
