using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Persistence.Configurations;

public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.ToTable("booking_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.BookingId).HasColumnName("booking_id");
        builder.Property(i => i.CourtId).HasColumnName("court_id");
        builder.Property(i => i.BookingDate).HasColumnName("booking_date");
        builder.Property(i => i.StartTime).HasColumnName("start_time");
        builder.Property(i => i.EndTime).HasColumnName("end_time");
        builder.Property(i => i.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
        builder.Property(i => i.CancelledAt).HasColumnName("cancelled_at");

        builder.HasOne(i => i.Court)
            .WithMany()
            .HasForeignKey(i => i.CourtId)
            .OnDelete(DeleteBehavior.Restrict);

        // Generated column that collapses to NULL once the slot is cancelled. MySQL unique
        // indexes allow unlimited NULLs, so a cancelled slot frees up for re-booking while an
        // active (Pending/Confirmed) one still blocks duplicates — this is the DB-level last
        // line of defense behind the application-level transaction lock in the booking engine.
        builder.Property<string?>("active_slot_key")
            .HasComputedColumnSql(
                "CASE WHEN `cancelled_at` IS NULL THEN CONCAT(`court_id`, '|', `booking_date`, '|', `start_time`) ELSE NULL END",
                stored: true);

        builder.HasIndex("active_slot_key").IsUnique();
    }
}
