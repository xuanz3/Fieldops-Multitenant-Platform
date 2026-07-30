using FieldOps.Domain.Customers;

namespace FieldOps.UnitTests;

public sealed class CustomerTests
{
    [Fact]
    public void Customer_preserves_tenant_ownership()
    {
        var tenantId = Guid.NewGuid();

        var customer = new Customer(
            tenantId,
            "client-001",
            "Example Client",
            "client@example.test");

        Assert.Equal(tenantId, customer.TenantId);
        Assert.Equal("CLIENT-001", customer.Reference);
    }

    [Fact]
    public void Customer_rejects_empty_tenant_id()
    {
        Assert.Throws<ArgumentException>(
            () => new Customer(
                Guid.Empty,
                "CLIENT-001",
                "Example Client"));
    }
}
