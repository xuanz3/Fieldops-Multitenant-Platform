namespace FieldOps.Application.Auditing;

public interface IAuditActorContext
{
    Guid? UserId { get; }

    string DisplayName { get; }

    string Role { get; }
}
