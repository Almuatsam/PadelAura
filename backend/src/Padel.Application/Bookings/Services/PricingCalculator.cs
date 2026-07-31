using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Bookings.Services;

public static class PricingCalculator
{
    /// <summary>
    /// Applies the highest-qualifying active promotion tier (largest MinimumHours &lt;= totalHours)
    /// per docs/02-TDD.md §5.2. subtotal already reflects the applicable rate; total mirrors
    /// subtotal (matches Booking.ApplyPricing setting Total = Subtotal).
    /// </summary>
    public static (decimal Subtotal, decimal Discount, decimal Total) Calculate(
        int totalHours,
        decimal rawSubtotal,
        IReadOnlyList<Promotion> activePromotions,
        DateOnly today)
    {
        var applicableRule = activePromotions
            .Where(p => p.IsActive
                && (p.StartDate is null || p.StartDate <= today)
                && (p.EndDate is null || p.EndDate >= today))
            .SelectMany(p => p.Rules)
            .Where(r => r.MinimumHours <= totalHours)
            .OrderByDescending(r => r.MinimumHours)
            .FirstOrDefault();

        if (applicableRule is null)
        {
            return (rawSubtotal, 0m, rawSubtotal);
        }

        var subtotal = applicableRule.DiscountType switch
        {
            DiscountType.FixedRate => totalHours * applicableRule.DiscountValue,
            DiscountType.Percentage => rawSubtotal * (1 - applicableRule.DiscountValue / 100m),
            _ => rawSubtotal,
        };

        var discount = rawSubtotal - subtotal;

        return (subtotal, discount, subtotal);
    }
}
