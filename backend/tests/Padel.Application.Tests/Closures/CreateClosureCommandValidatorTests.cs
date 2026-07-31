using FluentAssertions;
using Padel.Application.Closures;

namespace Padel.Application.Tests.Closures;

public sealed class CreateClosureCommandValidatorTests
{
    private readonly CreateClosureCommandValidator _validator = new();

    [Fact]
    public void Validate_Succeeds_WhenTimesAreOmitted()
    {
        var command = new CreateClosureCommand(null, new DateOnly(2026, 8, 1), null, null, "Full day");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Fails_WhenEndTimeIsNotAfterStartTime()
    {
        var command = new CreateClosureCommand(
            null, new DateOnly(2026, 8, 1), new TimeOnly(12, 0), new TimeOnly(10, 0), null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Fails_WhenCourtIdIsNotPositive()
    {
        var command = new CreateClosureCommand([0], new DateOnly(2026, 8, 1), null, null, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
