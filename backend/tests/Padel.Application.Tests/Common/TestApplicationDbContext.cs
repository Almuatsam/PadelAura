using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Padel.Application.Bookings;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;
using Padel.Domain.Enums;
using Padel.Infrastructure.Persistence;

namespace Padel.Application.Tests.Common;

/// <summary>
/// Wraps an InMemory PadelDbContext but replaces the one method that needs a real relational
/// provider (the FOR UPDATE locking query) with a plain LINQ equivalent — InMemory has no raw-SQL
/// support and no real row locking to exercise anyway; that part is verified live against MySQL
/// instead (see the Phase 3 plan's verification section).
/// </summary>
public sealed class TestApplicationDbContext(PadelDbContext inner) : IApplicationDbContext
{
    public DbSet<Admin> Admins => inner.Admins;
    public DbSet<Court> Courts => inner.Courts;
    public DbSet<CourtSchedule> CourtSchedules => inner.CourtSchedules;
    public DbSet<CourtClosure> CourtClosures => inner.CourtClosures;
    public DbSet<Customer> Customers => inner.Customers;
    public DbSet<Booking> Bookings => inner.Bookings;
    public DbSet<BookingItem> BookingItems => inner.BookingItems;
    public DbSet<Promotion> Promotions => inner.Promotions;
    public DbSet<PricingRule> PricingRules => inner.PricingRules;
    public DbSet<Payment> Payments => inner.Payments;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        inner.SaveChangesAsync(cancellationToken);

    // The InMemory provider has no real transactions (and throws on BeginTransactionAsync unless
    // that warning is suppressed) — a no-op stands in since there's no real locking to exercise
    // against InMemory anyway; SaveChangesAsync below still commits state immediately as it always
    // does, which is exactly what tests need to assert against.
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IDbContextTransaction>(new NoOpTransaction());

    public async Task<List<long>> GetCourtIdsWithActiveBookingForUpdateAsync(
        IReadOnlyList<long> courtIds,
        DateOnly date,
        TimeOnly startTime,
        CancellationToken cancellationToken)
    {
        var graceThreshold = DateTime.UtcNow.AddMinutes(-BookingPolicy.PendingPaymentGraceMinutes);

        // Mirrors PadelDbContext's reclaim-then-select: actually cancel stale Pending bookings
        // (not just exclude them from the read) so a fresh booking can reuse that same court/slot
        // without tripping the active_slot_key uniqueness guard.
        var staleBookings = await inner.BookingItems
            .Where(i => courtIds.Contains(i.CourtId) && i.BookingDate == date && i.StartTime == startTime
                && i.CancelledAt == null && i.Booking!.Status == BookingStatus.Pending
                && i.Booking.CreatedAt <= graceThreshold)
            .Select(i => i.Booking!)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var booking in staleBookings)
        {
            booking.Cancel();
        }

        if (staleBookings.Count > 0)
        {
            await inner.SaveChangesAsync(cancellationToken);
        }

        return await inner.BookingItems
            .Where(i => courtIds.Contains(i.CourtId) && i.BookingDate == date
                && i.StartTime == startTime && i.CancelledAt == null)
            .Select(i => i.CourtId)
            .ToListAsync(cancellationToken);
    }

    private sealed class NoOpTransaction : IDbContextTransaction
    {
        public Guid TransactionId { get; } = Guid.NewGuid();

        public void Commit() { }

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Rollback() { }

        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Dispose() { }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
