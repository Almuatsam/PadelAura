using Padel.Domain.Common;

namespace Padel.Domain.Entities;

/// <summary>
/// An exceptional closure. CourtId == null means every court is closed for the given window.
/// </summary>
public class CourtClosure : Entity
{
    public long? CourtId { get; private set; }
    public Court? Court { get; private set; }
    public DateOnly ClosureDate { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public string? Reason { get; private set; }

    private CourtClosure() { }

    public CourtClosure(long? courtId, DateOnly closureDate, TimeOnly? startTime, TimeOnly? endTime, string? reason)
    {
        CourtId = courtId;
        ClosureDate = closureDate;
        StartTime = startTime;
        EndTime = endTime;
        Reason = reason;
    }
}
