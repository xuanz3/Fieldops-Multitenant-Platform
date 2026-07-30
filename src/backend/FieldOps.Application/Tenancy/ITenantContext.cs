namespace FieldOps.Application.Tenancy;

public interface ITenantContext
{
    Guid? TenantId { get; }
}
