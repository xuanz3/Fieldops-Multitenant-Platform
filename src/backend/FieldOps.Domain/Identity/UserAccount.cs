using System.Net.Mail;
using FieldOps.Domain.Common;

namespace FieldOps.Domain.Identity;

public sealed class UserAccount
{
    private UserAccount()
    {
    }

    public UserAccount(
        Guid tenantId,
        string email,
        string displayName,
        string passwordHash,
        UserRole role,
        Guid? id = null,
        DateTimeOffset? createdAt = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));
        }

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(
                nameof(role),
                "A supported user role is required.");
        }

        Id = id ?? Guid.NewGuid();
        if (Id == Guid.Empty)
        {
            throw new ArgumentException(
                "User account ID cannot be empty.",
                nameof(id));
        }

        TenantId = tenantId;
        Email = NormaliseEmail(email);
        DisplayName = DomainText.Required(
            displayName,
            nameof(displayName),
            120);
        PasswordHash = DomainText.Required(
            passwordHash,
            nameof(passwordHash),
            512);
        Role = role;
        IsActive = true;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static string NormaliseEmail(string value)
    {
        var normalised = DomainText.Required(
            value,
            nameof(value),
            254).ToLowerInvariant();

        try
        {
            var address = new MailAddress(normalised);
            if (!string.Equals(
                    address.Address,
                    normalised,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException();
            }
        }
        catch (FormatException)
        {
            throw new ArgumentException(
                "A valid email address is required.",
                nameof(value));
        }

        return normalised;
    }
}
