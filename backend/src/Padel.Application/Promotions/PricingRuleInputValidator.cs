using FluentValidation;

namespace Padel.Application.Promotions;

public sealed class PricingRuleInputValidator : AbstractValidator<PricingRuleInput>
{
    public PricingRuleInputValidator()
    {
        RuleFor(x => x.MinimumHours).GreaterThan(0);
        RuleFor(x => x.DiscountValue).GreaterThan(0);
    }
}
