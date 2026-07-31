using Padel.Domain.Common;

namespace Padel.Domain.Entities;

public class Promotion : Entity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    private readonly List<PricingRule> _rules = [];
    public IReadOnlyCollection<PricingRule> Rules => _rules.AsReadOnly();

    private Promotion() { }

    public Promotion(string name, bool isActive, DateOnly? startDate, DateOnly? endDate)
    {
        Name = name;
        IsActive = isActive;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void Update(string name, DateOnly? startDate, DateOnly? endDate)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void ReplaceRules(IEnumerable<PricingRule> rules)
    {
        _rules.Clear();
        _rules.AddRange(rules);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
