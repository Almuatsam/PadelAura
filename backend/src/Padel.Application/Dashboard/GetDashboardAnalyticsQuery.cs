using MediatR;

namespace Padel.Application.Dashboard;

/// <summary>Range is "week" (last 7 days) or "month" (last 30 days), validated by
/// <see cref="GetDashboardAnalyticsQueryValidator"/>.</summary>
public sealed record GetDashboardAnalyticsQuery(string Range) : IRequest<DashboardAnalyticsDto>;
