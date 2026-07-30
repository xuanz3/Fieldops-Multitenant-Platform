using FieldOps.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.IntegrationTests;

[Collection("PostgreSQL integration")]
public sealed class TenantConstraintTests
{
    private readonly PostgreSqlDatabaseFixture _database;

    public TenantConstraintTests(
        PostgreSqlDatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task Same_reference_is_allowed_in_different_tenants()
    {
        await using var northside =
            _database.CreateDbContext(
                PostgreSqlDatabaseFixture.NorthsideTenantId);

        await using var bayside =
            _database.CreateDbContext(
                PostgreSqlDatabaseFixture.BaysideTenantId);

        Assert.NotNull(
            await northside.WorkOrders
                .SingleOrDefaultAsync(
                    workOrder =>
                        workOrder.Reference == "WO-1001"));

        Assert.NotNull(
            await bayside.WorkOrders
                .SingleOrDefaultAsync(
                    workOrder =>
                        workOrder.Reference == "WO-1001"));
    }

    [Fact]
    public async Task Duplicate_reference_is_rejected_inside_one_tenant()
    {
        await using var northside =
            _database.CreateDbContext(
                PostgreSqlDatabaseFixture.NorthsideTenantId);

        northside.WorkOrders.Add(
            new WorkOrder(
                PostgreSqlDatabaseFixture.NorthsideTenantId,
                PostgreSqlDatabaseFixture.NorthsideCustomerId,
                "WO-1001",
                "Duplicate reference test"));

        await Assert.ThrowsAsync<DbUpdateException>(
            () => northside.SaveChangesAsync());
    }

    [Fact]
    public async Task Cross_tenant_customer_relationship_is_rejected()
    {
        await using var northside =
            _database.CreateDbContext(
                PostgreSqlDatabaseFixture.NorthsideTenantId);

        northside.WorkOrders.Add(
            new WorkOrder(
                PostgreSqlDatabaseFixture.NorthsideTenantId,
                PostgreSqlDatabaseFixture.BaysideCustomerId,
                "WO-CROSS-TENANT",
                "Invalid cross-tenant relationship test"));

        await Assert.ThrowsAsync<DbUpdateException>(
            () => northside.SaveChangesAsync());
    }
}
