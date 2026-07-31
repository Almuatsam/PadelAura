using FluentValidation;

namespace Padel.Application.Promotions;

public sealed class UpdatePromotionCommandValidator : AbstractValidator<UpdatePromotionCommand>
{
    public UpdatePromotionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.Rules).SetValidator(new PricingRuleInputValidator());
    }
}
