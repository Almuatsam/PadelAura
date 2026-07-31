using FluentAssertions;
using Padel.Application.Courts.Queries.GetCourts;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;

namespace Padel.Application.Tests.Courts;

public sealed class GetCourtsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCourtsOrderedById_WithSchedules()
    {
        await using var context = TestDbContextFactory.Create();
        var courtB = new Court("Court B", 12m);
        var courtA = new Court("Court A", 15m);
        context.Courts.AddRange(courtB, courtA);
        await context.SaveChangesAsync(CancellationToken.None);

        courtA.ReplaceSchedules([new CourtSchedule(courtA.Id, 0, new TimeOnly(8, 0), new TimeOnly(23, 0))]);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCourtsQueryHandler(context);

        var result = await handler.Handle(new GetCourtsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(courtB.Id);
        result[1].Schedules.Should().ContainSingle();
    }
}
