using Microsoft.EntityFrameworkCore;
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
}
