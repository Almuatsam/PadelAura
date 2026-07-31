using Padel.Domain.Common;
using Padel.Domain.Enums;

namespace Padel.Domain.Entities;

public class Court : Entity
{
    public string Name { get; private set; } = string.Empty;
    public decimal HourPrice { get; private set; }
    public CourtStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<CourtSchedule> _schedules = [];
    public IReadOnlyCollection<CourtSchedule> Schedules => _schedules.AsReadOnly();

    private readonly List<CourtClosure> _closures = [];
    public IReadOnlyCollection<CourtClosure> Closures => _closures.AsReadOnly();

    private Court() { }

    public Court(string name, decimal hourPrice, CourtStatus status = CourtStatus.Active)
    {
        Name = name;
        HourPrice = hourPrice;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, decimal hourPrice, CourtStatus status)
    {
        Name = name;
        HourPrice = hourPrice;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceSchedules(IEnumerable<CourtSchedule> schedules)
    {
        _schedules.Clear();
        _schedules.AddRange(schedules);
        UpdatedAt = DateTime.UtcNow;
    }
}
