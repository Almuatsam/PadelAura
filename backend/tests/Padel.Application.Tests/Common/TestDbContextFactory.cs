using Microsoft.EntityFrameworkCore;
using Padel.Infrastructure.Persistence;

namespace Padel.Application.Tests.Common;

public static class TestDbContextFactory
{
    public static PadelDbContext Create()
    {
        var options = new DbContextOptionsBuilder<PadelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PadelDbContext(options);
    }
}
