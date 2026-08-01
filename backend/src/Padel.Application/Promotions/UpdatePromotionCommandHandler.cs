using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Exceptions;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;

namespace Padel.Application.Promotions;

public sealed class UpdatePromotionCommandHandler(IApplicationDbContext context, ICurrentAdminService currentAdmin)
    : IRequestHandler<UpdatePromotionCommand, PromotionDto>
{
    public async Task<PromotionDto> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await context.Promotions
            .Include(p => p.Rules)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Promotion), request.Id);

        promotion.Update(request.Name, request.StartDate, request.EndDate);
        promotion.SetActive(request.IsActive);
        promotion.ReplaceRules(request.Rules.Select(r =>
            new PricingRule(promotion.Id, r.MinimumHours, r.DiscountType, r.DiscountValue)));

        context.AuditLogs.Add(new AuditLog(
            currentAdmin.AdminId, "Update", nameof(Promotion), promotion.Id,
            JsonSerializer.Serialize(new { promotion.Name, promotion.IsActive })));

        await context.SaveChangesAsync(cancellationToken);

        return GetPromotionsQueryHandler.ToDto(promotion);
    }
}
