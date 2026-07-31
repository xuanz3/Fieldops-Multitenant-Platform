using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FieldOps.Domain.Common;

namespace FieldOps.Domain.Auditing;

public sealed class AuditEvent
{
    public const string GenesisHash =
        "GENESIS";

    private AuditEvent()
    {
    }

    public AuditEvent(
        Guid tenantId,
        long sequence,
        string action,
        string entityType,
        Guid entityId,
        string summary,
        string actorDisplayName,
        string actorRole,
        string previousHash,
        Guid? actorUserId = null,
        Guid? workOrderId = null,
        Guid? id = null,
        DateTimeOffset? occurredAt = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "Tenant ID cannot be empty.",
                nameof(tenantId));
        }

        if (sequence < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequence));
        }

        if (entityId == Guid.Empty)
        {
            throw new ArgumentException(
                "Entity ID cannot be empty.",
                nameof(entityId));
        }

        if (actorUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Actor user ID cannot be empty.",
                nameof(actorUserId));
        }

        if (workOrderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Work order ID cannot be empty.",
                nameof(workOrderId));
        }

        Id = id ?? Guid.NewGuid();

        if (Id == Guid.Empty)
        {
            throw new ArgumentException(
                "Audit event ID cannot be empty.",
                nameof(id));
        }

        TenantId = tenantId;
        Sequence = sequence;
        Action =
            DomainText.Required(
                action,
                nameof(action),
                80);
        EntityType =
            DomainText.Required(
                entityType,
                nameof(entityType),
                80);
        EntityId = entityId;
        WorkOrderId = workOrderId;
        Summary =
            DomainText.Required(
                summary,
                nameof(summary),
                1000);
        ActorUserId = actorUserId;
        ActorDisplayName =
            DomainText.Required(
                actorDisplayName,
                nameof(actorDisplayName),
                120);
        ActorRole =
            DomainText.Required(
                actorRole,
                nameof(actorRole),
                40);
        OccurredAt =
            occurredAt ??
            DateTimeOffset.UtcNow;
        PreviousHash =
            string.IsNullOrWhiteSpace(previousHash)
                ? GenesisHash
                : previousHash.Trim();
        EventHash =
            CalculateHash(
                TenantId,
                Sequence,
                Action,
                EntityType,
                EntityId,
                Summary,
                ActorDisplayName,
                ActorRole,
                PreviousHash,
                ActorUserId,
                WorkOrderId,
                OccurredAt);
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public long Sequence { get; private set; }

    public string Action { get; private set; } =
        string.Empty;

    public string EntityType { get; private set; } =
        string.Empty;

    public Guid EntityId { get; private set; }

    public Guid? WorkOrderId { get; private set; }

    public string Summary { get; private set; } =
        string.Empty;

    public Guid? ActorUserId { get; private set; }

    public string ActorDisplayName { get; private set; } =
        string.Empty;

    public string ActorRole { get; private set; } =
        string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public string PreviousHash { get; private set; } =
        string.Empty;

    public string EventHash { get; private set; } =
        string.Empty;

    public bool HasValidHash() =>
        string.Equals(
            EventHash,
            CalculateHash(
                TenantId,
                Sequence,
                Action,
                EntityType,
                EntityId,
                Summary,
                ActorDisplayName,
                ActorRole,
                PreviousHash,
                ActorUserId,
                WorkOrderId,
                OccurredAt),
            StringComparison.Ordinal);

    private static string CalculateHash(
        Guid tenantId,
        long sequence,
        string action,
        string entityType,
        Guid entityId,
        string summary,
        string actorDisplayName,
        string actorRole,
        string previousHash,
        Guid? actorUserId,
        Guid? workOrderId,
        DateTimeOffset occurredAt)
    {
        var canonical = string.Join(
            "|",
            tenantId.ToString("D"),
            sequence.ToString(
                CultureInfo.InvariantCulture),
            occurredAt
                .ToUniversalTime()
                .ToString(
                    "O",
                    CultureInfo.InvariantCulture),
            actorUserId?.ToString("D") ?? "-",
            actorDisplayName,
            actorRole,
            action,
            entityType,
            entityId.ToString("D"),
            workOrderId?.ToString("D") ?? "-",
            summary,
            previousHash);

        return Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    canonical)));
    }
}
