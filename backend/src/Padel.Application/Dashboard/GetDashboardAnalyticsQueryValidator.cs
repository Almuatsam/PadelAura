using FluentValidation;

namespace Padel.Application.Dashboard;

public sealed class GetDashboardAnalyticsQueryValidator : AbstractValidator<GetDashboardAnalyticsQuery>
{
    public GetDashboardAnalyticsQueryValidator()
    {
        RuleFor(x => x.Range)
            .Must(range => range is "week" or "month")
            .WithMessage("Range must be 'week' or 'month'.");
    }
}
