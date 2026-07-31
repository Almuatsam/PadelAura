using MediatR;

namespace Padel.Application.Dashboard;

public sealed record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
