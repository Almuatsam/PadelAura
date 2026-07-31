using FluentValidation;
using Padel.Application.Courts;

namespace Padel.Application.Courts.Commands.UpdateCourt;

public sealed class UpdateCourtCommandValidator : AbstractValidator<UpdateCourtCommand>
{
    public UpdateCourtCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.HourPrice)
            .GreaterThan(0);

        RuleFor(x => x.Status).IsInEnum();

        RuleForEach(x => x.Schedules)
            .SetValidator(new CourtScheduleInputValidator());
    }
}
