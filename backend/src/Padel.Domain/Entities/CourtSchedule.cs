using Padel.Domain.Common;

namespace Padel.Domain.Entities;

public class CourtSchedule : Entity
{
    public long CourtId { get; private set; }
    public Court? Court { get; private set; }
    public int DayOfWeek { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }

    private CourtSchedule() { }

    public CourtSchedule(long courtId, int dayOfWeek, TimeOnly openTime, TimeOnly closeTime)
    {
        if (dayOfWeek is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "Day of week must be between 0 (Sunday) and 6 (Saturday).");
        }

        CourtId = courtId;
        DayOfWeek = dayOfWeek;
        OpenTime = openTime;
        CloseTime = closeTime;
    }
}
