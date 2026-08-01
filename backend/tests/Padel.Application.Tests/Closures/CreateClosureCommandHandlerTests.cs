using FluentAssertions;
using Padel.Application.Closures;
using Padel.Application.Common.Exceptions;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;

namespace Padel.Application.Tests.Closures;

public sealed class CreateClosureCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesSingleNullCourtClosure_WhenCourtIdsIsEmpty()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreateClosureCommandHandler(context, new FakeCurrentAdminService());
        var command = new CreateClosureCommand([], new DateOnly(2026, 8, 1), null, null, "Maintenance");

        var ids = await handler.Handle(command, CancellationToken.None);

        ids.Should().ContainSingle();
        context.CourtClosures.Should().ContainSingle(c => c.CourtId == null);
        context.AuditLogs.Should().ContainSingle(a => a.Action == "Create" && a.EntityType == "CourtClosure");
    }

    [Fact]
    public async Task Handle_CreatesOneClosurePerCourt_WhenCourtIdsIsProvided()
    {
        await using var context = TestDbContextFactory.Create();
        var courtA = new Court("Court A", 15m);
        var courtB = new Court("Court B", 15m);
        context.Courts.AddRange(courtA, courtB);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateClosureCommandHandler(context, new FakeCurrentAdminService());
        var command = new CreateClosureCommand(
            [courtA.Id, courtB.Id], new DateOnly(2026, 8, 1), new TimeOnly(10, 0), new TimeOnly(12, 0), null);

        var ids = await handler.Handle(command, CancellationToken.None);

        ids.Should().HaveCount(2);
        context.CourtClosures.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenACourtIdDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new CreateClosureCommandHandler(context, new FakeCurrentAdminService());
        var command = new CreateClosureCommand([999], new DateOnly(2026, 8, 1), null, null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
