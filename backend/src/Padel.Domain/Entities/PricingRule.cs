using Padel.Domain.Common;
using Padel.Domain.Enums;

namespace Padel.Domain.Entities;

public class PricingRule : Entity
{
    public long PromotionId { get; private set; }
    public Promotion? Promotion { get; private set; }
    public int MinimumHours { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }

    private PricingRule() { }

    public PricingRule(long promotionId, int minimumHours, DiscountType discountType, decimal discountValue)
    {
        PromotionId = promotionId;
        MinimumHours = minimumHours;
        DiscountType = discountType;
        DiscountValue = discountValue;
    }
}
