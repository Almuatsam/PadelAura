using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Bookings.Services;

/// <summary>
/// Pure schedule/closure logic shared by availability lookup and booking creation. Takes
/// already-loaded courts and the closures for a single date (including CourtId == null rows,
/// which mean "all courts" and are NOT reachable through Court.Closures since that navigation
/// is keyed by the CourtId foreign key).
/// </summary>
public static class SlotAvailabilityCalculator
{
    public static IReadOnlyList<Court> GetEligibleCourts(
        IReadOnlyList<Court> courts,
        IReadOnlyList<CourtClosure> closuresForDate,
        int dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        var eligible = new List<Court>();

        foreach (var court in courts)
        {
            if (court.Status != CourtStatus.Active)
            {
                continue;
            }

            var schedule = court.Schedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);
            if (schedule is null || startTime < schedule.OpenTime || endTime > schedule.CloseTime)
            {
                continue;
            }

            var isClosed = closuresForDate
                .Where(c => c.CourtId is null || c.CourtId == court.Id)
                .Any(c => Overlaps(startTime, endTime, c.StartTime, c.EndTime));

            if (!isClosed)
            {
                eligible.Add(court);
            }
        }

        return eligible;
    }

    private static bool Overlaps(TimeOnly slotStart, TimeOnly slotEnd, TimeOnly? closureStart, TimeOnly? closureEnd)
    {
        var start = closureStart ?? TimeOnly.MinValue;
        var end = closureEnd ?? TimeOnly.MaxValue;

        return slotStart < end && start < slotEnd;
    }
}
