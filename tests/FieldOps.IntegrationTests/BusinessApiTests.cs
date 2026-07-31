using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FieldOps.IntegrationTests;

[Collection("PostgreSQL integration")]
public sealed class BusinessApiTests
    : IDisposable
{
    private readonly FieldOpsApiFactory _factory;

    public BusinessApiTests(
        PostgreSqlDatabaseFixture database)
    {
        _factory =
            new FieldOpsApiFactory(
                database.ConnectionString);
    }

    [Fact]
    public async Task Customer_list_requires_authentication()
    {
        using var client =
            _factory.CreateClient();

        var response =
            await client.GetAsync(
                "/api/customers");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Dispatcher_lists_only_signed_tenant_customers()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var response =
            await client.GetAsync(
                "/api/customers?page=1&pageSize=100");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document =
            await ReadJsonAsync(response);

        var items =
            document.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .ToList();

        Assert.Contains(
            items,
            item =>
                item.GetProperty("id")
                    .GetGuid() ==
                PostgreSqlDatabaseFixture
                    .NorthsideCustomerId);

        Assert.DoesNotContain(
            items,
            item =>
                item.GetProperty("id")
                    .GetGuid() ==
                PostgreSqlDatabaseFixture
                    .BaysideCustomerId);
    }

    [Fact]
    public async Task Dispatcher_can_create_and_update_customer()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var reference =
            $"CLIENT-{Guid.NewGuid():N}"[..18];

        var createResponse =
            await client.PostAsJsonAsync(
                "/api/customers",
                new
                {
                    reference,
                    name = "API Test Customer",
                    email = "api-customer@example.test"
                });

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        using var created =
            await ReadJsonAsync(createResponse);

        var customerId =
            created.RootElement
                .GetProperty("id")
                .GetGuid();

        var updateResponse =
            await client.PutAsJsonAsync(
                $"/api/customers/{customerId}",
                new
                {
                    name = "Updated API Customer",
                    email = "updated-api@example.test"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        using var updated =
            await ReadJsonAsync(updateResponse);

        Assert.Equal(
            "Updated API Customer",
            updated.RootElement
                .GetProperty("name")
                .GetString());
    }

    [Fact]
    public async Task Duplicate_customer_reference_returns_conflict()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var response =
            await client.PostAsJsonAsync(
                "/api/customers",
                new
                {
                    reference = "CLIENT-001",
                    name = "Duplicate",
                    email = "duplicate@example.test"
                });

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    [Fact]
    public async Task Cross_tenant_customer_is_hidden()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var response =
            await client.GetAsync(
                $"/api/customers/{PostgreSqlDatabaseFixture.BaysideCustomerId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task Technician_cannot_manage_customers()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideTechnicianEmail);

        var response =
            await client.GetAsync(
                "/api/customers");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Dispatcher_can_create_and_read_work_order()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var reference =
            $"WO-{Guid.NewGuid():N}"[..18];

        var createResponse =
            await client.PostAsJsonAsync(
                "/api/work-orders",
                new
                {
                    customerId =
                        PostgreSqlDatabaseFixture
                            .NorthsideCustomerId,
                    reference,
                    title = "API test work order",
                    description =
                        "Fictional integration test.",
                    priority = "High"
                });

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        using var created =
            await ReadJsonAsync(createResponse);

        var workOrderId =
            created.RootElement
                .GetProperty("id")
                .GetGuid();

        var detailResponse =
            await client.GetAsync(
                $"/api/work-orders/{workOrderId}");

        Assert.Equal(
            HttpStatusCode.OK,
            detailResponse.StatusCode);

        using var detail =
            await ReadJsonAsync(detailResponse);

        Assert.Equal(
            "High",
            detail.RootElement
                .GetProperty("priority")
                .GetString());
    }

    [Fact]
    public async Task Cross_tenant_customer_cannot_be_used_for_work_order()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var response =
            await client.PostAsJsonAsync(
                "/api/work-orders",
                new
                {
                    customerId =
                        PostgreSqlDatabaseFixture
                            .BaysideCustomerId,
                    reference =
                        $"WO-{Guid.NewGuid():N}"[..18],
                    title = "Blocked cross tenant order",
                    description = "Must not be created.",
                    priority = "Normal"
                });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task Cross_tenant_work_order_is_hidden()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var response =
            await client.GetAsync(
                $"/api/work-orders/{PostgreSqlDatabaseFixture.BaysideWorkOrderId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task Work_order_filters_return_matching_records()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var response =
            await client.GetAsync(
                "/api/work-orders?status=Submitted&priority=High&pageSize=100");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document =
            await ReadJsonAsync(response);

        var items =
            document.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .ToList();

        Assert.NotEmpty(items);

        Assert.All(
            items,
            item =>
            {
                Assert.Equal(
                    "Submitted",
                    item.GetProperty("status")
                        .GetString());
                Assert.Equal(
                    "High",
                    item.GetProperty("priority")
                        .GetString());
            });
    }

    [Fact]
    public async Task Work_order_update_rejects_stale_version()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var createResponse =
            await client.PostAsJsonAsync(
                "/api/work-orders",
                new
                {
                    customerId =
                        PostgreSqlDatabaseFixture
                            .NorthsideCustomerId,
                    reference =
                        $"WO-{Guid.NewGuid():N}"[..18],
                    title = "Concurrency test",
                    description = "Version one",
                    priority = "Normal"
                });

        createResponse.EnsureSuccessStatusCode();

        using var created =
            await ReadJsonAsync(createResponse);

        var workOrderId =
            created.RootElement
                .GetProperty("id")
                .GetGuid();

        var firstUpdate =
            await client.PutAsJsonAsync(
                $"/api/work-orders/{workOrderId}",
                new
                {
                    customerId =
                        PostgreSqlDatabaseFixture
                            .NorthsideCustomerId,
                    title = "Concurrency test updated",
                    description = "Version two",
                    priority = "Urgent",
                    version = 1
                });

        Assert.Equal(
            HttpStatusCode.OK,
            firstUpdate.StatusCode);

        var staleUpdate =
            await client.PutAsJsonAsync(
                $"/api/work-orders/{workOrderId}",
                new
                {
                    customerId =
                        PostgreSqlDatabaseFixture
                            .NorthsideCustomerId,
                    title = "Stale update",
                    description = "Must be rejected",
                    priority = "Low",
                    version = 1
                });

        Assert.Equal(
            HttpStatusCode.Conflict,
            staleUpdate.StatusCode);
    }

    [Fact]
    public async Task Client_cannot_manage_work_orders()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideClientEmail);

        var response =
            await client.GetAsync(
                "/api/work-orders");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Tenant_header_cannot_override_business_api()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        client.DefaultRequestHeaders.Add(
            "X-Tenant-Id",
            PostgreSqlDatabaseFixture
                .BaysideTenantId
                .ToString());

        var response =
            await client.GetAsync(
                "/api/customers?pageSize=100");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document =
            await ReadJsonAsync(response);

        var items =
            document.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .ToList();

        Assert.DoesNotContain(
            items,
            item =>
                item.GetProperty("id")
                    .GetGuid() ==
                PostgreSqlDatabaseFixture
                    .BaysideCustomerId);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private async Task<HttpClient>
        CreateAuthenticatedClientAsync(
            string email)
    {
        var client =
            _factory.CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "/api/auth/login",
                new
                {
                    tenantSlug =
                        PostgreSqlDatabaseFixture
                            .NorthsideTenantSlug,
                    email,
                    password =
                        PostgreSqlDatabaseFixture
                            .TestPassword
                });

        response.EnsureSuccessStatusCode();

        using var document =
            await ReadJsonAsync(response);

        var token =
            document.RootElement
                .GetProperty("accessToken")
                .GetString();

        Assert.False(
            string.IsNullOrWhiteSpace(token));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        return client;
    }

    private static async Task<JsonDocument>
        ReadJsonAsync(
            HttpResponseMessage response)
    {
        var content =
            await response.Content
                .ReadAsStringAsync();

        return JsonDocument.Parse(content);
    }
}
