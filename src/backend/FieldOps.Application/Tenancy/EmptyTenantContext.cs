namespace FieldOps.Application.Tenancy;

public sealed class EmptyTenantContext : ITenantContext
{
    public Guid? TenantId => null;
}
