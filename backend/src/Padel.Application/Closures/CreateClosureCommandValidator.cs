using FluentValidation;

namespace Padel.Application.Closures;

public sealed class CreateClosureCommandValidator : AbstractValidator<CreateClosureCommand>
{
    public CreateClosureCommandValidator()
    {
        When(x => x.StartTime is not null && x.EndTime is not null, () =>
        {
            RuleFor(x => x.EndTime!.Value)
                .GreaterThan(x => x.StartTime!.Value)
                .WithMessage("endTime must be after startTime.");
        });

        RuleFor(x => x.CourtIds)
            .Must(ids => ids is null || ids.Count == 0 || ids.All(id => id > 0))
            .WithMessage("courtIds must contain only positive ids.");
    }
}
