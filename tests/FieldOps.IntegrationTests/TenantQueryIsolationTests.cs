using Microsoft.EntityFrameworkCore;

namespace FieldOps.IntegrationTests;

[Collection("PostgreSQL integration")]
public sealed class TenantQueryIsolationTests
{
    private readonly PostgreSqlDatabaseFixture _database;

    public TenantQueryIsolationTests(
        PostgreSqlDatabaseFixture database)
    {
        _database = database;
    }

    [Fact]
    public async Task Tenant_queries_return_only_owned_records()
    {
        await using var northside =
            _database.CreateDbContext(
                PostgreSqlDatabaseFixture.NorthsideTenantId);

        var customerReferences =
            await northside.Customers
                .OrderBy(customer => customer.Reference)
                .Select(customer => customer.Reference)
                .ToListAsync();

        var workOrderReferences =
            await northside.WorkOrders
                .OrderBy(workOrder => workOrder.Reference)
                .Select(workOrder => workOrder.Reference)
                .ToListAsync();

        Assert.Equal(
            new[] { "CLIENT-001", "CLIENT-002" },
            customerReferences);

        Assert.Equal(
            new[] { "WO-1001", "WO-1002" },
            workOrderReferences);
    }

    [Fact]
    public async Task Missing_tenant_context_returns_no_business_rows()
    {
        await using var missingTenant =
            _database.CreateDbContext(null);

        Assert.Equal(
            0,
            await missingTenant.Customers.CountAsync());

        Assert.Equal(
            0,
            await missingTenant.WorkOrders.CountAsync());
    }

    [Fact]
    public async Task Tenant_cannot_fetch_another_tenants_work_order()
    {
        await using var northside =
            _database.CreateDbContext(
                PostgreSqlDatabaseFixture.NorthsideTenantId);

        var hiddenWorkOrder =
            await northside.WorkOrders
                .SingleOrDefaultAsync(
                    workOrder =>
                        workOrder.Id ==
                        PostgreSqlDatabaseFixture.BaysideWorkOrderId);

        Assert.Null(hiddenWorkOrder);
    }
}
