using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Persistence.Configurations;

public class CourtScheduleConfiguration : IEntityTypeConfiguration<CourtSchedule>
{
    public void Configure(EntityTypeBuilder<CourtSchedule> builder)
    {
        builder.ToTable("court_schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CourtId).HasColumnName("court_id");

        builder.Property(s => s.DayOfWeek)
            .HasColumnName("day_of_week")
            .HasColumnType("tinyint");

        builder.Property(s => s.OpenTime).HasColumnName("open_time");
        builder.Property(s => s.CloseTime).HasColumnName("close_time");

        builder.HasIndex(s => new { s.CourtId, s.DayOfWeek });
    }
}
