using FluentAssertions;
using Padel.Application.Closures;
using Padel.Application.Common;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;

namespace Padel.Application.Tests.Closures;

public sealed class GetClosuresQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsUpcomingClosures_WithCourtNameAndExcludingPastOnes()
    {
        await using var context = TestDbContextFactory.Create();
        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);

        var today = DateOnly.FromDateTime(OmanClock.Now());

        context.CourtClosures.AddRange(
            new CourtClosure(court.Id, today.AddDays(5), new TimeOnly(10, 0), new TimeOnly(12, 0), "Maintenance"),
            new CourtClosure(null, today.AddDays(2), null, null, "Public holiday"),
            new CourtClosure(court.Id, today.AddDays(-3), null, null, "Old closure"));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetClosuresQueryHandler(context);

        var closures = await handler.Handle(new GetClosuresQuery(), CancellationToken.None);

        closures.Should().HaveCount(2);
        closures.Should().BeInAscendingOrder(c => c.ClosureDate);
        closures.Should().Contain(c => c.CourtId == court.Id && c.CourtName == "Court A" && c.Reason == "Maintenance");
        closures.Should().Contain(c => c.CourtId == null && c.CourtName == null && c.Reason == "Public holiday");
    }
}
