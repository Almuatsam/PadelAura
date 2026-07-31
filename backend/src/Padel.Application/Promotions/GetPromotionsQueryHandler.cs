using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Interfaces;

namespace Padel.Application.Promotions;

public sealed class GetPromotionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPromotionsQuery, List<PromotionDto>>
{
    public async Task<List<PromotionDto>> Handle(GetPromotionsQuery request, CancellationToken cancellationToken)
    {
        var promotions = await context.Promotions
            .Include(p => p.Rules)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        return promotions.Select(ToDto).ToList();
    }

    internal static PromotionDto ToDto(Padel.Domain.Entities.Promotion promotion) => new(
        promotion.Id,
        promotion.Name,
        promotion.IsActive,
        promotion.StartDate,
        promotion.EndDate,
        promotion.Rules
            .OrderBy(r => r.MinimumHours)
            .Select(r => new PricingRuleDto(r.MinimumHours, r.DiscountType.ToString(), r.DiscountValue))
            .ToList());
}
