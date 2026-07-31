using FluentValidation;

namespace Padel.Application.Promotions;

public sealed class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.Rules).SetValidator(new PricingRuleInputValidator());
    }
}
