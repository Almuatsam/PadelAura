using FluentValidation;
using Padel.Application.Courts;

namespace Padel.Application.Courts.Commands.CreateCourt;

public sealed class CreateCourtCommandValidator : AbstractValidator<CreateCourtCommand>
{
    public CreateCourtCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.HourPrice)
            .GreaterThan(0);

        RuleForEach(x => x.Schedules)
            .SetValidator(new CourtScheduleInputValidator());
    }
}
