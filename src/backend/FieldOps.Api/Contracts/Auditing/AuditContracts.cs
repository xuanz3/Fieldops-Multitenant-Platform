namespace FieldOps.Api.Contracts.Auditing;

public sealed record AuditEventResponse(
    Guid Id,
    long Sequence,
    string Action,
    string EntityType,
    Guid EntityId,
    Guid? WorkOrderId,
    string Summary,
    Guid? ActorUserId,
    string ActorDisplayName,
    string ActorRole,
    DateTimeOffset OccurredAt,
    string PreviousHash,
    string EventHash);

public sealed record AuditVerificationResponse(
    bool IsValid,
    int EventCount,
    long? FirstSequence,
    long? LastSequence,
    string? Failure);
