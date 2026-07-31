using Padel.Domain.Common;

namespace Padel.Domain.Entities;

public class Customer : Entity
{
    public string Phone { get; private set; } = string.Empty;
    public string? FullName { get; private set; }
    public string? Email { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Customer() { }

    public Customer(string phone, string? fullName, string? email)
    {
        Phone = phone;
        FullName = fullName;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateContactInfo(string? fullName, string? email)
    {
        FullName = fullName ?? FullName;
        Email = email ?? Email;
    }
}
