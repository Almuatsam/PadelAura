using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Courts;
using Padel.Application.Courts.Commands.CreateCourt;
using Padel.Application.Tests.Common;

namespace Padel.Application.Tests.Courts;

public sealed class CreateCourtCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsCourtWithSchedules()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreateCourtCommandHandler(context);

        var command = new CreateCourtCommand(
            "Court D",
            15.5m,
            [new CourtScheduleInput(0, new TimeOnly(8, 0), new TimeOnly(23, 0))]);

        var id = await handler.Handle(command, CancellationToken.None);

        var court = await context.Courts
            .Include(c => c.Schedules)
            .FirstAsync(c => c.Id == id, CancellationToken.None);

        court.Name.Should().Be("Court D");
        court.HourPrice.Should().Be(15.5m);
        court.Schedules.Should().ContainSingle(s =>
            s.DayOfWeek == 0 && s.OpenTime == new TimeOnly(8, 0) && s.CloseTime == new TimeOnly(23, 0));
    }
}
