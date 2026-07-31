namespace Padel.Application.Dashboard;

public sealed record DashboardSummaryDto(int TodayBookingsCount, decimal TodayRevenue, double OccupancyRate);
