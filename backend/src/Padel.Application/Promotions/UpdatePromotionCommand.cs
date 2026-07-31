using MediatR;

namespace Padel.Application.Promotions;

public sealed record UpdatePromotionCommand(
    long Id,
    string Name,
    bool IsActive,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyList<PricingRuleInput> Rules) : IRequest<PromotionDto>;
