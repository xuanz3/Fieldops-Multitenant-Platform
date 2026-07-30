using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FieldOps.IntegrationTests;

[Collection("PostgreSQL integration")]
public sealed class AuthenticationApiTests
    : IDisposable
{
    private readonly FieldOpsApiFactory _factory;

    public AuthenticationApiTests(
        PostgreSqlDatabaseFixture database)
    {
        _factory = new FieldOpsApiFactory(
            database.ConnectionString);
    }

    [Fact]
    public async Task Valid_login_returns_token_and_tenant_identity()
    {
        using var client = _factory.CreateClient();

        var response = await LoginAsync(
            client,
            PostgreSqlDatabaseFixture.NorthsideTenantSlug,
            PostgreSqlDatabaseFixture.NorthsideAdminEmail,
            PostgreSqlDatabaseFixture.TestPassword);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document =
            await ReadJsonAsync(response);

        Assert.False(
            string.IsNullOrWhiteSpace(
                document.RootElement
                    .GetProperty("accessToken")
                    .GetString()));

        Assert.Equal(
            PostgreSqlDatabaseFixture.NorthsideTenantId,
            document.RootElement
                .GetProperty("user")
                .GetProperty("tenantId")
                .GetGuid());

        Assert.Equal(
            "TenantAdmin",
            document.RootElement
                .GetProperty("user")
                .GetProperty("role")
                .GetString());
    }

    [Fact]
    public async Task Invalid_password_returns_unauthorised()
    {
        using var client = _factory.CreateClient();

        var response = await LoginAsync(
            client,
            PostgreSqlDatabaseFixture.NorthsideTenantSlug,
            PostgreSqlDatabaseFixture.NorthsideAdminEmail,
            "Wrong-Password-2026!");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Tenant_slug_prevents_cross_tenant_login()
    {
        using var client = _factory.CreateClient();

        var response = await LoginAsync(
            client,
            PostgreSqlDatabaseFixture.BaysideTenantSlug,
            PostgreSqlDatabaseFixture.NorthsideAdminEmail,
            PostgreSqlDatabaseFixture.TestPassword);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Me_requires_authentication()
    {
        using var client = _factory.CreateClient();

        var response =
            await client.GetAsync("/api/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Me_returns_the_signed_tenant_identity()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture.NorthsideAdminEmail);

        var response =
            await client.GetAsync("/api/auth/me");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document =
            await ReadJsonAsync(response);

        Assert.Equal(
            PostgreSqlDatabaseFixture.NorthsideTenantId,
            document.RootElement
                .GetProperty("tenantId")
                .GetGuid());
    }

    [Fact]
    public async Task Tenant_admin_can_access_admin_policy()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture.NorthsideAdminEmail);

        var response =
            await client.GetAsync(
                "/api/authorisation/admin");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task Dispatcher_can_access_dispatch_policy()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture.NorthsideDispatcherEmail);

        var response =
            await client.GetAsync(
                "/api/authorisation/dispatch");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task Technician_is_forbidden_from_dispatch_policy()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture.NorthsideTechnicianEmail);

        var response =
            await client.GetAsync(
                "/api/authorisation/dispatch");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Client_is_forbidden_from_admin_policy()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture.NorthsideClientEmail);

        var response =
            await client.GetAsync(
                "/api/authorisation/admin");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Malformed_token_is_rejected()
    {
        using var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "not-a-valid-jwt");

        var response =
            await client.GetAsync(
                "/api/authorisation/tenant-summary");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Tenant_header_cannot_override_signed_token()
    {
        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture.NorthsideAdminEmail);

        client.DefaultRequestHeaders.Add(
            "X-Tenant-Id",
            PostgreSqlDatabaseFixture.BaysideTenantId.ToString());

        var response =
            await client.GetAsync(
                "/api/authorisation/tenant-summary");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document =
            await ReadJsonAsync(response);

        Assert.Equal(
            PostgreSqlDatabaseFixture.NorthsideTenantId,
            document.RootElement
                .GetProperty("tenantId")
                .GetGuid());

        Assert.Equal(
            2,
            document.RootElement
                .GetProperty("customerCount")
                .GetInt32());

        Assert.Equal(
            2,
            document.RootElement
                .GetProperty("workOrderCount")
                .GetInt32());
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private async Task<HttpClient>
        CreateAuthenticatedClientAsync(
            string email)
    {
        var client = _factory.CreateClient();

        var response = await LoginAsync(
            client,
            PostgreSqlDatabaseFixture.NorthsideTenantSlug,
            email,
            PostgreSqlDatabaseFixture.TestPassword);

        response.EnsureSuccessStatusCode();

        using var document =
            await ReadJsonAsync(response);

        var token = document.RootElement
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

    private static Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string tenantSlug,
        string email,
        string password)
    {
        return client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                tenantSlug,
                email,
                password
            });
    }

    private static async Task<JsonDocument> ReadJsonAsync(
        HttpResponseMessage response)
    {
        var content =
            await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(content);
    }
}
