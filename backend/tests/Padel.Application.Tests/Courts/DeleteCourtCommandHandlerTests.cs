using FluentAssertions;
using Padel.Application.Common.Exceptions;
using Padel.Application.Courts.Commands.DeleteCourt;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;

namespace Padel.Application.Tests.Courts;

public sealed class DeleteCourtCommandHandlerTests
{
    [Fact]
    public async Task Handle_RemovesCourt_WhenItExists()
    {
        await using var context = TestDbContextFactory.Create();
        var court = new Court("Court A", 15m);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteCourtCommandHandler(context, new FakeCurrentAdminService());

        await handler.Handle(new DeleteCourtCommand(court.Id), CancellationToken.None);

        context.Courts.Should().BeEmpty();
        context.AuditLogs.Should().ContainSingle(a => a.Action == "Delete" && a.EntityType == "Court" && a.EntityId == court.Id);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenCourtDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();
        var handler = new DeleteCourtCommandHandler(context, new FakeCurrentAdminService());

        var act = () => handler.Handle(new DeleteCourtCommand(999), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
