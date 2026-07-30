using FieldOps.Domain.Tenants;

namespace FieldOps.UnitTests;

public sealed class TenantTests
{
    [Fact]
    public void Tenant_normalises_slug()
    {
        var tenant = new Tenant("Northside Services", "northside-services");

        Assert.Equal("northside-services", tenant.Slug);
    }

    [Theory]
    [InlineData("")]
    [InlineData("North Side")]
    [InlineData("-northside")]
    [InlineData("northside-")]
    public void Tenant_rejects_invalid_slug(string slug)
    {
        Assert.Throws<ArgumentException>(
            () => new Tenant("Northside Services", slug));
    }
}
