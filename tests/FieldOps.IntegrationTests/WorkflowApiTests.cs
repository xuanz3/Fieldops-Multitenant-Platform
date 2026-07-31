using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FieldOps.IntegrationTests;

[Collection(
    "Workflow API PostgreSQL integration")]
public sealed class WorkflowApiTests
    : IDisposable
{
    private static readonly Guid
        NorthsideTechnicianUserId =
            Guid.Parse(
                "11111111-1111-1111-1111-111111130003");

    private static readonly Guid
        NorthsideClientUserId =
            Guid.Parse(
                "11111111-1111-1111-1111-111111130004");

    private static readonly Guid
        BaysideAdminUserId =
            Guid.Parse(
                "22222222-2222-2222-2222-222222230001");

    private readonly FieldOpsApiFactory _factory;

    public WorkflowApiTests(
        PostgreSqlDatabaseFixture database)
    {
        _factory =
            new FieldOpsApiFactory(
                database.ConnectionString);
    }

    [Fact]
    public async Task Workflow_queues_require_authentication()
    {
        using var client =
            _factory.CreateClient();

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (
                await client.GetAsync(
                    "/api/technician/work-orders")
            ).StatusCode);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (
                await client.GetAsync(
                    "/api/client/work-orders")
            ).StatusCode);
    }

    [Fact]
    public async Task Dispatcher_can_list_tenant_technicians_and_clients()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var technicians =
            await dispatcher.GetAsync(
                "/api/workflow/technicians");

        Assert.Equal(
            HttpStatusCode.OK,
            technicians.StatusCode);

        using var technicianJson =
            await ReadJsonAsync(technicians);

        Assert.Contains(
            technicianJson.RootElement
                .EnumerateArray(),
            item =>
                item.GetProperty("id")
                    .GetGuid() ==
                NorthsideTechnicianUserId);

        var clients =
            await dispatcher.GetAsync(
                "/api/workflow/clients");

        Assert.Equal(
            HttpStatusCode.OK,
            clients.StatusCode);

        using var clientJson =
            await ReadJsonAsync(clients);

        Assert.Contains(
            clientJson.RootElement
                .EnumerateArray(),
            item =>
                item.GetProperty("id")
                    .GetGuid() ==
                NorthsideClientUserId);
    }

    [Fact]
    public async Task Dispatcher_can_assign_and_technician_can_start()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var workOrder =
            await CreateWorkOrderAsync(
                dispatcher,
                "Assignment start flow");

        var assigned =
            await dispatcher.PostAsJsonAsync(
                $"/api/workflow/work-orders/{workOrder.Id}/assign",
                new
                {
                    technicianUserId =
                        NorthsideTechnicianUserId,
                    version =
                        workOrder.Version
                });

        Assert.Equal(
            HttpStatusCode.OK,
            assigned.StatusCode);

        using var assignedJson =
            await ReadJsonAsync(assigned);

        Assert.Equal(
            "Assigned",
            assignedJson.RootElement
                .GetProperty("status")
                .GetString());

        var assignedVersion =
            assignedJson.RootElement
                .GetProperty("version")
                .GetInt64();

        using var technician =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideTechnicianEmail);

        var started =
            await technician.PostAsJsonAsync(
                $"/api/technician/work-orders/{workOrder.Id}/start",
                new
                {
                    version =
                        assignedVersion
                });

        Assert.Equal(
            HttpStatusCode.OK,
            started.StatusCode);

        using var startedJson =
            await ReadJsonAsync(started);

        Assert.Equal(
            "InProgress",
            startedJson.RootElement
                .GetProperty("status")
                .GetString());
    }

    [Fact]
    public async Task Full_workflow_reaches_client_approved_completion()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var workOrder =
            await CreateWorkOrderAsync(
                dispatcher,
                "Full approval flow");

        var assigned =
            await dispatcher.PostAsJsonAsync(
                $"/api/workflow/work-orders/{workOrder.Id}/assign",
                new
                {
                    technicianUserId =
                        NorthsideTechnicianUserId,
                    version =
                        workOrder.Version
                });

        using var assignedJson =
            await ReadJsonAsync(assigned);

        using var technician =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideTechnicianEmail);

        var started =
            await technician.PostAsJsonAsync(
                $"/api/technician/work-orders/{workOrder.Id}/start",
                new
                {
                    version =
                        assignedJson.RootElement
                            .GetProperty("version")
                            .GetInt64()
                });

        using var startedJson =
            await ReadJsonAsync(started);

        var submitted =
            await technician.PostAsJsonAsync(
                $"/api/technician/work-orders/{workOrder.Id}/submit",
                new
                {
                    completionSummary =
                        "Work completed and safely tested.",
                    version =
                        startedJson.RootElement
                            .GetProperty("version")
                            .GetInt64()
                });

        Assert.Equal(
            HttpStatusCode.OK,
            submitted.StatusCode);

        using var submittedJson =
            await ReadJsonAsync(submitted);

        Assert.Equal(
            "AwaitingClientApproval",
            submittedJson.RootElement
                .GetProperty("status")
                .GetString());

        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideClientEmail);

        var queue =
            await client.GetAsync(
                "/api/client/work-orders");

        using var queueJson =
            await ReadJsonAsync(queue);

        Assert.Contains(
            queueJson.RootElement
                .EnumerateArray(),
            item =>
                item.GetProperty("id")
                    .GetGuid() ==
                workOrder.Id);

        var approved =
            await client.PostAsJsonAsync(
                $"/api/client/work-orders/{workOrder.Id}/approve",
                new
                {
                    version =
                        submittedJson.RootElement
                            .GetProperty("version")
                            .GetInt64()
                });

        Assert.Equal(
            HttpStatusCode.OK,
            approved.StatusCode);

        using var approvedJson =
            await ReadJsonAsync(approved);

        Assert.Equal(
            "Completed",
            approvedJson.RootElement
                .GetProperty("status")
                .GetString());
    }

    [Fact]
    public async Task Client_can_reopen_and_return_work_to_dispatch()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var workOrder =
            await CreateWorkOrderAsync(
                dispatcher,
                "Reopen flow");

        var awaiting =
            await MoveToAwaitingApprovalAsync(
                dispatcher,
                workOrder);

        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideClientEmail);

        var reopened =
            await client.PostAsJsonAsync(
                $"/api/client/work-orders/{workOrder.Id}/reopen",
                new
                {
                    reason =
                        "The original issue is still visible.",
                    version =
                        awaiting.Version
                });

        Assert.Equal(
            HttpStatusCode.OK,
            reopened.StatusCode);

        using var reopenedJson =
            await ReadJsonAsync(reopened);

        Assert.Equal(
            "Reopened",
            reopenedJson.RootElement
                .GetProperty("status")
                .GetString());

        Assert.Equal(
            JsonValueKind.Null,
            reopenedJson.RootElement
                .GetProperty(
                    "assignedTechnicianId")
                .ValueKind);
    }

    [Fact]
    public async Task Cross_tenant_or_wrong_role_assignments_are_blocked()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var workOrder =
            await CreateWorkOrderAsync(
                dispatcher,
                "Blocked assignment");

        var crossTenant =
            await dispatcher.PostAsJsonAsync(
                $"/api/workflow/work-orders/{workOrder.Id}/assign",
                new
                {
                    technicianUserId =
                        BaysideAdminUserId,
                    version =
                        workOrder.Version
                });

        Assert.Equal(
            HttpStatusCode.NotFound,
            crossTenant.StatusCode);

        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideClientEmail);

        var forbidden =
            await client.PostAsJsonAsync(
                $"/api/workflow/work-orders/{workOrder.Id}/assign",
                new
                {
                    technicianUserId =
                        NorthsideTechnicianUserId,
                    version =
                        workOrder.Version
                });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            forbidden.StatusCode);
    }

    [Fact]
    public async Task Unassigned_technician_cannot_start_work()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var workOrder =
            await CreateWorkOrderAsync(
                dispatcher,
                "Unassigned start");

        using var technician =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideTechnicianEmail);

        var response =
            await technician.PostAsJsonAsync(
                $"/api/technician/work-orders/{workOrder.Id}/start",
                new
                {
                    version =
                        workOrder.Version
                });

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private async Task<WorkflowRecord>
        MoveToAwaitingApprovalAsync(
            HttpClient dispatcher,
            WorkflowRecord workOrder)
    {
        var assigned =
            await dispatcher.PostAsJsonAsync(
                $"/api/workflow/work-orders/{workOrder.Id}/assign",
                new
                {
                    technicianUserId =
                        NorthsideTechnicianUserId,
                    version =
                        workOrder.Version
                });

        using var assignedJson =
            await ReadJsonAsync(assigned);

        using var technician =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideTechnicianEmail);

        var started =
            await technician.PostAsJsonAsync(
                $"/api/technician/work-orders/{workOrder.Id}/start",
                new
                {
                    version =
                        assignedJson.RootElement
                            .GetProperty("version")
                            .GetInt64()
                });

        using var startedJson =
            await ReadJsonAsync(started);

        var submitted =
            await technician.PostAsJsonAsync(
                $"/api/technician/work-orders/{workOrder.Id}/submit",
                new
                {
                    completionSummary =
                        "Completed for Client review.",
                    version =
                        startedJson.RootElement
                            .GetProperty("version")
                            .GetInt64()
                });

        using var submittedJson =
            await ReadJsonAsync(submitted);

        return new WorkflowRecord(
            workOrder.Id,
            submittedJson.RootElement
                .GetProperty("version")
                .GetInt64());
    }

    private async Task<WorkflowRecord>
        CreateWorkOrderAsync(
            HttpClient dispatcher,
            string title)
    {
        var reference =
            $"WO-{Guid.NewGuid():N}"[..20];

        var response =
            await dispatcher.PostAsJsonAsync(
                "/api/work-orders",
                new
                {
                    customerId =
                        PostgreSqlDatabaseFixture
                            .NorthsideCustomerId,
                    reference,
                    title,
                    description =
                        "Fictional workflow integration test.",
                    priority =
                        "Normal"
                });

        response.EnsureSuccessStatusCode();

        using var document =
            await ReadJsonAsync(response);

        return new WorkflowRecord(
            document.RootElement
                .GetProperty("id")
                .GetGuid(),
            document.RootElement
                .GetProperty("version")
                .GetInt64());
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

    private sealed record WorkflowRecord(
        Guid Id,
        long Version);
}
