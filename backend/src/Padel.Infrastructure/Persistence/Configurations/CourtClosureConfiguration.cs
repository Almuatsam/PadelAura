using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Persistence.Configurations;

public class CourtClosureConfiguration : IEntityTypeConfiguration<CourtClosure>
{
    public void Configure(EntityTypeBuilder<CourtClosure> builder)
    {
        builder.ToTable("court_closures");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CourtId).HasColumnName("court_id");
        builder.Property(c => c.ClosureDate).HasColumnName("closure_date");
        builder.Property(c => c.StartTime).HasColumnName("start_time");
        builder.Property(c => c.EndTime).HasColumnName("end_time");

        builder.Property(c => c.Reason)
            .HasColumnName("reason")
            .HasMaxLength(255);

        builder.HasIndex(c => new { c.CourtId, c.ClosureDate });
    }
}
