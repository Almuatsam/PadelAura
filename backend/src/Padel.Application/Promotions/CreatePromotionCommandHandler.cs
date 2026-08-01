using System.Text.Json;
using MediatR;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Promotions;

public sealed class CreatePromotionCommandHandler(IApplicationDbContext context, ICurrentAdminService currentAdmin)
    : IRequestHandler<CreatePromotionCommand, long>
{
    public async Task<long> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = new Promotion(request.Name, request.IsActive, request.StartDate, request.EndDate);

        context.Promotions.Add(promotion);

        // Save first so `promotion.Id` is populated before it's used as the FK for the rule rows.
        await context.SaveChangesAsync(cancellationToken);

        promotion.ReplaceRules(request.Rules.Select(r =>
            new PricingRule(promotion.Id, r.MinimumHours, r.DiscountType, r.DiscountValue)));

        context.AuditLogs.Add(new AuditLog(
            currentAdmin.AdminId, "Create", nameof(Promotion), promotion.Id,
            JsonSerializer.Serialize(new { promotion.Name, promotion.IsActive })));

        await context.SaveChangesAsync(cancellationToken);

        return promotion.Id;
    }
}
