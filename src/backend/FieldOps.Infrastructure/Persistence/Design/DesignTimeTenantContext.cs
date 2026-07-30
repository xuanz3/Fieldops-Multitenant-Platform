using FieldOps.Application.Tenancy;

namespace FieldOps.Infrastructure.Persistence.Design;

internal sealed class DesignTimeTenantContext : ITenantContext
{
    public Guid? TenantId => null;
}
