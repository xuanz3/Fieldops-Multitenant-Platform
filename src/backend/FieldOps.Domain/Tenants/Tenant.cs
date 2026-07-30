using FieldOps.Domain.Common;

namespace FieldOps.Domain.Tenants;

public sealed class Tenant
{
    private Tenant()
    {
    }

    public Tenant(
        string name,
        string slug,
        Guid? id = null,
        DateTimeOffset? createdAt = null)
    {
        Id = id ?? Guid.NewGuid();
        if (Id == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(id));
        }

        Name = DomainText.Required(name, nameof(name), 160);
        Slug = NormaliseSlug(slug);
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    private static string NormaliseSlug(string value)
    {
        var slug = DomainText.Required(value, nameof(value), 80).ToLowerInvariant();

        if (slug.StartsWith('-') ||
            slug.EndsWith('-') ||
            slug.Any(character =>
                !char.IsLower(character) && !char.IsDigit(character) && character != '-'))
        {
            throw new ArgumentException(
                "Tenant slug must contain lowercase letters, numbers or internal hyphens.",
                nameof(value));
        }

        return slug;
    }
}
