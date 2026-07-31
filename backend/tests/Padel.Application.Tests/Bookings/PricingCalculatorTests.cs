using FluentAssertions;
using Padel.Application.Bookings.Services;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Bookings;

public sealed class PricingCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 8, 1);

    private static Promotion CreatePromotion(bool isActive, DateOnly? start, DateOnly? end, params PricingRule[] rules)
    {
        var promotion = new Promotion("Promo", isActive, start, end);
        promotion.ReplaceRules(rules);
        return promotion;
    }

    [Fact]
    public void Calculate_AppliesFixedRateForLowestQualifyingTier()
    {
        var promotion = CreatePromotion(
            true, null, null,
            new PricingRule(1, 1, DiscountType.FixedRate, 10m),
            new PricingRule(1, 2, DiscountType.FixedRate, 8m));

        var (subtotal, discount, total) = PricingCalculator.Calculate(1, 12m, [promotion], Today);

        subtotal.Should().Be(10m);
        discount.Should().Be(2m);
        total.Should().Be(10m);
    }

    [Fact]
    public void Calculate_PicksHighestQualifyingTier_ForMoreHours()
    {
        var promotion = CreatePromotion(
            true, null, null,
            new PricingRule(1, 1, DiscountType.FixedRate, 10m),
            new PricingRule(1, 2, DiscountType.FixedRate, 8m));

        var (subtotal, discount, total) = PricingCalculator.Calculate(3, 36m, [promotion], Today);

        subtotal.Should().Be(24m);
        discount.Should().Be(12m);
        total.Should().Be(24m);
    }

    [Fact]
    public void Calculate_AppliesPercentageDiscount()
    {
        var promotion = CreatePromotion(
            true, null, null,
            new PricingRule(1, 1, DiscountType.Percentage, 20m));

        var (subtotal, discount, total) = PricingCalculator.Calculate(1, 100m, [promotion], Today);

        subtotal.Should().Be(80m);
        discount.Should().Be(20m);
        total.Should().Be(80m);
    }

    [Fact]
    public void Calculate_IgnoresInactivePromotion()
    {
        var promotion = CreatePromotion(
            false, null, null,
            new PricingRule(1, 1, DiscountType.FixedRate, 10m));

        var (subtotal, discount, total) = PricingCalculator.Calculate(1, 12m, [promotion], Today);

        subtotal.Should().Be(12m);
        discount.Should().Be(0m);
        total.Should().Be(12m);
    }

    [Fact]
    public void Calculate_IgnoresExpiredPromotion()
    {
        var promotion = CreatePromotion(
            true, new DateOnly(2026, 1, 1), new DateOnly(2026, 7, 1),
            new PricingRule(1, 1, DiscountType.FixedRate, 10m));

        var (subtotal, discount, total) = PricingCalculator.Calculate(1, 12m, [promotion], Today);

        subtotal.Should().Be(12m);
        discount.Should().Be(0m);
        total.Should().Be(12m);
    }

    [Fact]
    public void Calculate_ReturnsRawSubtotal_WhenNoTierQualifies()
    {
        var promotion = CreatePromotion(
            true, null, null,
            new PricingRule(1, 2, DiscountType.FixedRate, 8m));

        var (subtotal, discount, total) = PricingCalculator.Calculate(1, 12m, [promotion], Today);

        subtotal.Should().Be(12m);
        discount.Should().Be(0m);
        total.Should().Be(12m);
    }
}
