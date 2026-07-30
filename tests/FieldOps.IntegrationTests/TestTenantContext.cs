using FieldOps.Application.Tenancy;

namespace FieldOps.IntegrationTests;

internal sealed class TestTenantContext : ITenantContext
{
    public TestTenantContext(Guid? tenantId)
    {
        TenantId = tenantId;
    }

    public Guid? TenantId { get; }
}
