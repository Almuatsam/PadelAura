using Padel.Domain.Enums;

namespace Padel.Application.Promotions;

public sealed record PricingRuleInput(int MinimumHours, DiscountType DiscountType, decimal DiscountValue);
