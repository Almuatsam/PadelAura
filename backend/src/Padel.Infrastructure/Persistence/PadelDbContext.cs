using Microsoft.EntityFrameworkCore;

namespace Padel.Infrastructure.Persistence;

public class PadelDbContext(DbContextOptions<PadelDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadelDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
