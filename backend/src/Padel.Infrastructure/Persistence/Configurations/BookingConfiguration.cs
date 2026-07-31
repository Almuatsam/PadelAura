using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookingReference)
            .HasColumnName("booking_reference")
            .HasMaxLength(12)
            .IsRequired();

        builder.HasIndex(b => b.BookingReference).IsUnique();

        builder.Property(b => b.CustomerId).HasColumnName("customer_id");

        builder.Property(b => b.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(10,2)");
        builder.Property(b => b.Discount).HasColumnName("discount").HasColumnType("decimal(10,2)");
        builder.Property(b => b.Total).HasColumnName("total").HasColumnType("decimal(10,2)");

        builder.Property(b => b.PaymentMethod)
            .HasColumnName("payment_method")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.PaymentStatus)
            .HasColumnName("payment_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.CreatedAt).HasColumnName("created_at");

        builder.Metadata.FindNavigation(nameof(Booking.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Booking)
            .HasForeignKey(i => i.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.Status);
    }
}
