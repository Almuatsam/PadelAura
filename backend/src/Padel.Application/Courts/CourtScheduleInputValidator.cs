using FluentValidation;

namespace Padel.Application.Courts;

public sealed class CourtScheduleInputValidator : AbstractValidator<CourtScheduleInput>
{
    public CourtScheduleInputValidator()
    {
        RuleFor(x => x.DayOfWeek).InclusiveBetween(0, 6);

        RuleFor(x => x.CloseTime)
            .GreaterThan(x => x.OpenTime)
            .WithMessage("closeTime must be after openTime.");
    }
}
