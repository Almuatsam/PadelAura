using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Padel.Domain.Entities;

namespace Padel.Infrastructure.Persistence.Configurations;

public class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRule>
{
    public void Configure(EntityTypeBuilder<PricingRule> builder)
    {
        builder.ToTable("pricing_rules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.PromotionId).HasColumnName("promotion_id");
        builder.Property(r => r.MinimumHours).HasColumnName("minimum_hours");

        builder.Property(r => r.DiscountType)
            .HasColumnName("discount_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.DiscountValue).HasColumnName("discount_value").HasColumnType("decimal(10,2)");

        builder.HasIndex(r => new { r.PromotionId, r.MinimumHours }).IsUnique();
    }
}
