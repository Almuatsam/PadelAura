using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Persistence;

public class PadelDbContext(DbContextOptions<PadelDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<CourtSchedule> CourtSchedules => Set<CourtSchedule>();
    public DbSet<CourtClosure> CourtClosures => Set<CourtClosure>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PadelDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        Database.BeginTransactionAsync(cancellationToken);

    public async Task<List<long>> GetCourtIdsWithActiveBookingForUpdateAsync(
        IReadOnlyList<long> courtIds,
        DateOnly date,
        TimeOnly startTime,
        CancellationToken cancellationToken)
    {
        if (courtIds.Count == 0)
        {
            return [];
        }

        // The id list is inlined as literal text (safe: these are `long`s we already fetched from
        // our own DB, never raw user input), while `date`/`startTime` go through SqlQueryRaw's own
        // `{0}`/`{1}` parameter placeholders. FOR UPDATE takes row/gap locks on this exact
        // court+date+start_time range so a concurrent request for the same slot blocks here and
        // re-reads fresh data instead of racing past this check.
        var idList = string.Join(",", courtIds);
        var sql =
            $"SELECT court_id AS Value FROM booking_items " +
            $"WHERE court_id IN ({idList}) AND booking_date = {{0}} AND start_time = {{1}} AND cancelled_at IS NULL " +
            "FOR UPDATE";

        return await Database.SqlQueryRaw<long>(sql, date, startTime).ToListAsync(cancellationToken);
    }
}
