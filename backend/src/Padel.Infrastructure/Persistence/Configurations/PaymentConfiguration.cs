using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.BookingId).HasColumnName("booking_id");

        builder.Property(p => p.Provider)
            .HasColumnName("provider")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.SessionId)
            .HasColumnName("session_id")
            .HasMaxLength(100);

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.TransactionReference)
            .HasColumnName("transaction_reference")
            .HasMaxLength(100);

        builder.Property(p => p.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");

        builder.HasOne(p => p.Booking)
            .WithMany()
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.BookingId);
        builder.HasIndex(p => p.SessionId);
    }
}
