using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Padel.Application.Bookings;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

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
        // our own DB, never raw user input); sorted so concurrent calls always request row locks
        // in the same order. `date`/`startTime` go through parameter placeholders. This guard turns
        // that "never raw user input" assumption into an enforced invariant — a valid database
        // primary key is always positive, so this fails loudly (in Release too, unlike
        // Debug.Assert) rather than silently interpolating something unexpected into the SQL text
        // if a future change ever threads unvalidated input through this path.
        if (courtIds.Any(id => id <= 0))
        {
            throw new ArgumentOutOfRangeException(nameof(courtIds), "Court ids must be positive primary keys.");
        }

        var idList = string.Join(",", courtIds.OrderBy(id => id));
        var graceThreshold = DateTime.UtcNow.AddMinutes(-BookingPolicy.PendingPaymentGraceMinutes);

        // A single lock-acquisition statement: FOR UPDATE locks every non-cancelled row for this
        // exact court+date+start_time in one shot (unconditionally on status), avoiding the
        // deadlocks a second, separately-ordered locking statement could cause under concurrent
        // load. Composing .Include() on top of FromSqlRaw is supported EF Core relational-provider
        // composition.
        var lockedItems = await BookingItems
            .FromSqlRaw(
                $"SELECT * FROM booking_items WHERE court_id IN ({idList}) AND booking_date = {{0}} " +
                "AND start_time = {1} AND cancelled_at IS NULL FOR UPDATE",
                date, startTime)
            .Include(i => i.Booking)
            .ToListAsync(cancellationToken);

        // Reclaim abandoned Pending bookings among the rows we just locked — cancelling them here
        // (rather than merely excluding them below) is what actually frees the DB-level unique
        // index guard for a fresh booking at that same court/slot. Safe from deadlocks: these exact
        // rows are already locked by this transaction from the query above, so this UPDATE doesn't
        // need to negotiate any new lock with anyone.
        var staleBookings = lockedItems
            .Where(i => i.Booking!.Status == BookingStatus.Pending && i.Booking.CreatedAt <= graceThreshold)
            .Select(i => i.Booking!)
            .Distinct()
            .ToList();

        foreach (var booking in staleBookings)
        {
            booking.Cancel();
        }

        if (staleBookings.Count > 0)
        {
            await SaveChangesAsync(cancellationToken);
        }

        return lockedItems
            .Where(i => i.Booking!.Status == BookingStatus.Confirmed
                || (i.Booking.Status == BookingStatus.Pending && i.Booking.CreatedAt > graceThreshold))
            .Select(i => i.CourtId)
            .ToList();
    }
}
