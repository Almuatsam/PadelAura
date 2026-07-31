using Padel.Domain.Common;
using Padel.Domain.Enums;

namespace Padel.Domain.Entities;

public class Admin : Entity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public AdminRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Admin() { }

    public Admin(string fullName, string email, string passwordHash, AdminRole role)
    {
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }
}
