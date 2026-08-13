namespace Padel.Application.Dashboard;

public sealed record DashboardAnalyticsDto(
    IReadOnlyList<DailyOccupancyDto> Occupancy,
    IReadOnlyList<DailyRevenueDto> Revenue,
    IReadOnlyList<BusiestHourDto> BusiestHours,
    decimal TotalRevenue,
    // 0-100, unlike DashboardSummaryDto.OccupancyRate which is 0-1 — this endpoint is newer and
    // returns a display-ready percentage; keep the distinction in mind when consuming both.
    double OccupancyRatePercent);

/// <summary>Booked slots (BookingItem rows), not distinct bookings, for a single calendar day.</summary>
public sealed record DailyOccupancyDto(DateOnly Date, int BookingsCount);

public sealed record DailyRevenueDto(DateOnly Date, decimal Revenue);

public sealed record BusiestHourDto(int Hour, int Count);
