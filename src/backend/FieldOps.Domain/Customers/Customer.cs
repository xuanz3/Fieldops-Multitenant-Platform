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
        DateTimeOffset? createdAt = null,
        Guid? clientUserId = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));
        }

        Id = id ?? Guid.NewGuid();
        if (Id == Guid.Empty)
        {
            throw new ArgumentException(
                "Customer ID cannot be empty.",
                nameof(id));
        }

        if (clientUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Client user ID cannot be empty.",
                nameof(clientUserId));
        }

        TenantId = tenantId;
        Reference =
            DomainText.Reference(
                reference,
                nameof(reference));
        Name =
            DomainText.Required(
                name,
                nameof(name),
                160);
        Email =
            DomainText.Optional(
                email,
                nameof(email),
                254);
        ClientUserId = clientUserId;
        CreatedAt =
            createdAt ??
            DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Reference { get; private set; } =
        string.Empty;

    public string Name { get; private set; } =
        string.Empty;

    public string? Email { get; private set; }

    public Guid? ClientUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public void UpdateContact(
        string name,
        string? email)
    {
        Name =
            DomainText.Required(
                name,
                nameof(name),
                160);
        Email =
            DomainText.Optional(
                email,
                nameof(email),
                254);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void LinkClient(
        Guid? clientUserId)
    {
        if (clientUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Client user ID cannot be empty.",
                nameof(clientUserId));
        }

        ClientUserId = clientUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
