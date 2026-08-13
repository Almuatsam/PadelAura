using FluentAssertions;
using Padel.Application.Dashboard;

namespace Padel.Application.Tests.Dashboard;

public sealed class GetDashboardAnalyticsQueryValidatorTests
{
    private readonly GetDashboardAnalyticsQueryValidator _validator = new();

    [Theory]
    [InlineData("week")]
    [InlineData("month")]
    public void Validate_Succeeds_ForKnownRangeValues(string range)
    {
        var result = _validator.Validate(new GetDashboardAnalyticsQuery(range));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("year")]
    [InlineData("")]
    [InlineData("Week")]
    public void Validate_Fails_ForUnknownRangeValue(string range)
    {
        var result = _validator.Validate(new GetDashboardAnalyticsQuery(range));

        result.IsValid.Should().BeFalse();
    }
}
