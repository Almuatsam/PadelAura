using MediatR;

namespace Padel.Application.Promotions;

public sealed record CreatePromotionCommand(
    string Name,
    bool IsActive,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyList<PricingRuleInput> Rules) : IRequest<long>;
