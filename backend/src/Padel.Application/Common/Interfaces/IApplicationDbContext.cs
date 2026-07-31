using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Padel.Domain.Entities;

namespace Padel.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Admin> Admins { get; }
    DbSet<Court> Courts { get; }
    DbSet<CourtSchedule> CourtSchedules { get; }
    DbSet<CourtClosure> CourtClosures { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<BookingItem> BookingItems { get; }
    DbSet<Promotion> Promotions { get; }
    DbSet<PricingRule> PricingRules { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Takes a row/gap lock (SELECT ... FOR UPDATE) on the given date/start-time within the
    /// candidate courts, returning which of them already have an active (non-cancelled) booking.
    /// Must be called inside a transaction started by <see cref="BeginTransactionAsync"/> — the
    /// lock is what serializes concurrent booking attempts for the same slot.
    /// </summary>
    Task<List<long>> GetCourtIdsWithActiveBookingForUpdateAsync(
        IReadOnlyList<long> courtIds,
        DateOnly date,
        TimeOnly startTime,
        CancellationToken cancellationToken);
}
