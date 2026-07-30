using FieldOps.Domain.Common;

namespace FieldOps.Domain.Customers;

public sealed class Customer
{
    private Customer()
    {
    }

    public Customer(
        Guid tenantId,
        string reference,
        string name,
        string? email = null,
        Guid? id = null,
        DateTimeOffset? createdAt = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        }

        Id = id ?? Guid.NewGuid();
        if (Id == Guid.Empty)
        {
            throw new ArgumentException("Customer ID cannot be empty.", nameof(id));
        }

        TenantId = tenantId;
        Reference = DomainText.Reference(reference, nameof(reference));
        Name = DomainText.Required(name, nameof(name), 160);
        Email = DomainText.Optional(email, nameof(email), 254);
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Reference { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void UpdateContact(string name, string? email)
    {
        Name = DomainText.Required(name, nameof(name), 160);
        Email = DomainText.Optional(email, nameof(email), 254);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
