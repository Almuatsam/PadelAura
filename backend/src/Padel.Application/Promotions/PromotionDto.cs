namespace Padel.Application.Promotions;

public sealed record PricingRuleDto(int MinimumHours, string DiscountType, decimal DiscountValue);

public sealed record PromotionDto(
    long Id,
    string Name,
    bool IsActive,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyList<PricingRuleDto> Rules);
