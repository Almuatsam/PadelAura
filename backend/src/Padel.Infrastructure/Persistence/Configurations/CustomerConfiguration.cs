using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(c => c.Phone).IsUnique();

        builder.Property(c => c.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(120);

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(150);

        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
    }
}
