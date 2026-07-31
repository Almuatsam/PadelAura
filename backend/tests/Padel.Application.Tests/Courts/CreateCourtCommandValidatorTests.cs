using FluentAssertions;
using Padel.Application.Courts;
using Padel.Application.Courts.Commands.CreateCourt;

namespace Padel.Application.Tests.Courts;

public sealed class CreateCourtCommandValidatorTests
{
    private readonly CreateCourtCommandValidator _validator = new();

    [Fact]
    public void Validate_Succeeds_ForValidCommand()
    {
        var command = new CreateCourtCommand(
            "Court A",
            15m,
            [new CourtScheduleInput(0, new TimeOnly(8, 0), new TimeOnly(23, 0))]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenNameIsEmpty()
    {
        var command = new CreateCourtCommand(string.Empty, 15m, []);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Fails_WhenHourPriceIsNotPositive()
    {
        var command = new CreateCourtCommand("Court A", 0m, []);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Fails_WhenScheduleDayOfWeekIsOutOfRange()
    {
        var command = new CreateCourtCommand(
            "Court A",
            15m,
            [new CourtScheduleInput(7, new TimeOnly(8, 0), new TimeOnly(23, 0))]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Fails_WhenCloseTimeIsNotAfterOpenTime()
    {
        var command = new CreateCourtCommand(
            "Court A",
            15m,
            [new CourtScheduleInput(0, new TimeOnly(10, 0), new TimeOnly(9, 0))]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
