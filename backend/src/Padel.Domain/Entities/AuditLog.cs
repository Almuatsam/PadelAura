using Padel.Domain.Common;

namespace Padel.Domain.Entities;

public class AuditLog : Entity
{
    public long AdminId { get; private set; }
    public Admin? Admin { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public long EntityId { get; private set; }
    public string? ChangesJson { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AuditLog() { }

    public AuditLog(long adminId, string action, string entityType, long entityId, string? changesJson)
    {
        AdminId = adminId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        ChangesJson = changesJson;
        CreatedAt = DateTime.UtcNow;
    }
}
