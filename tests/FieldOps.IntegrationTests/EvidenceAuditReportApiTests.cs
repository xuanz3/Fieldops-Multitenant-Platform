using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Npgsql;

namespace FieldOps.IntegrationTests;

[Collection(
    "Evidence API PostgreSQL integration")]
public sealed class EvidenceAuditReportApiTests
    : IDisposable
{
    private static readonly Guid
        NorthsideWorkOrderOne =
            Guid.Parse(
                "11111111-1111-1111-1111-111111120001");

    private readonly
        PostgreSqlDatabaseFixture
        _database;

    private readonly FieldOpsApiFactory
        _factory;

    public EvidenceAuditReportApiTests(
        PostgreSqlDatabaseFixture database)
    {
        _database = database;
        _factory =
            new FieldOpsApiFactory(
                database.ConnectionString);
    }

    [Fact]
    public async Task Dispatcher_can_upload_list_and_download_attachment()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var uploaded =
            await UploadTextAsync(
                dispatcher,
                NorthsideWorkOrderOne,
                "dispatcher-evidence.txt",
                "Evidence uploaded by Dispatcher.");

        Assert.Equal(
            "dispatcher-evidence.txt",
            uploaded.RootElement
                .GetProperty("fileName")
                .GetString());

        Assert.Equal(
            64,
            uploaded.RootElement
                .GetProperty("sha256")
                .GetString()!
                .Length);

        var attachmentId =
            uploaded.RootElement
                .GetProperty("id")
                .GetGuid();

        var list =
            await dispatcher.GetAsync(
                $"/api/work-orders/{NorthsideWorkOrderOne}/attachments");

        Assert.Equal(
            HttpStatusCode.OK,
            list.StatusCode);

        using var listJson =
            await ReadJsonAsync(list);

        Assert.Contains(
            listJson.RootElement
                .EnumerateArray(),
            item =>
                item.GetProperty("id")
                    .GetGuid() ==
                attachmentId);

        var download =
            await dispatcher.GetAsync(
                $"/api/work-orders/{NorthsideWorkOrderOne}/attachments/{attachmentId}");

        Assert.Equal(
            HttpStatusCode.OK,
            download.StatusCode);

        Assert.Equal(
            "Evidence uploaded by Dispatcher.",
            await download.Content
                .ReadAsStringAsync());
    }

    [Fact]
    public async Task Linked_client_can_download_but_cannot_upload()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var uploaded =
            await UploadTextAsync(
                dispatcher,
                NorthsideWorkOrderOne,
                "client-visible.txt",
                "Client-visible evidence.");

        var attachmentId =
            uploaded.RootElement
                .GetProperty("id")
                .GetGuid();

        using var client =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideClientEmail);

        var list =
            await client.GetAsync(
                $"/api/work-orders/{NorthsideWorkOrderOne}/attachments");

        Assert.Equal(
            HttpStatusCode.OK,
            list.StatusCode);

        var download =
            await client.GetAsync(
                $"/api/work-orders/{NorthsideWorkOrderOne}/attachments/{attachmentId}");

        Assert.Equal(
            HttpStatusCode.OK,
            download.StatusCode);

        using var uploadContent =
            CreateTextContent(
                "client-upload.txt",
                "Clients cannot upload.");

        var upload =
            await client.PostAsync(
                $"/api/work-orders/{NorthsideWorkOrderOne}/attachments",
                uploadContent);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            upload.StatusCode);
    }

    [Fact]
    public async Task Audit_chain_verifies_after_business_and_attachment_events()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        await UploadTextAsync(
            dispatcher,
            NorthsideWorkOrderOne,
            "audit-chain.txt",
            "Hash-chain test evidence.");

        var response =
            await dispatcher.GetAsync(
                "/api/audit-events/verify");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using var document =
            await ReadJsonAsync(response);

        Assert.True(
            document.RootElement
                .GetProperty("isValid")
                .GetBoolean());

        Assert.True(
            document.RootElement
                .GetProperty("eventCount")
                .GetInt32() >
            0);
    }

    [Fact]
    public async Task PostgreSql_trigger_rejects_audit_update()
    {
        await using var connection =
            new NpgsqlConnection(
                _database.ConnectionString);

        await connection.OpenAsync();

        await using var command =
            new NpgsqlCommand(
                """
                UPDATE audit_events
                SET "Summary" = 'tampered'
                WHERE "Id" = (
                    SELECT "Id"
                    FROM audit_events
                    ORDER BY "Sequence"
                    LIMIT 1
                );
                """,
                connection);

        var exception =
            await Assert.ThrowsAsync<
                PostgresException>(
                () =>
                    command
                        .ExecuteNonQueryAsync());

        Assert.Equal(
            "P0001",
            exception.SqlState);

        Assert.Contains(
            "append-only",
            exception.Message
                .ToLowerInvariant());
    }

    [Fact]
    public async Task Dispatcher_can_read_and_export_operations_report()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        var report =
            await dispatcher.GetAsync(
                "/api/reports/operations");

        Assert.Equal(
            HttpStatusCode.OK,
            report.StatusCode);

        using var document =
            await ReadJsonAsync(report);

        Assert.True(
            document.RootElement
                .GetProperty(
                    "totalWorkOrders")
                .GetInt32() >
            0);

        Assert.True(
            document.RootElement
                .GetProperty(
                    "auditEventCount")
                .GetInt32() >
            0);

        var csv =
            await dispatcher.GetAsync(
                "/api/reports/operations.csv");

        Assert.Equal(
            HttpStatusCode.OK,
            csv.StatusCode);

        Assert.StartsWith(
            "text/csv",
            csv.Content.Headers
                .ContentType?
                .MediaType ??
            string.Empty);

        Assert.Contains(
            "Total work orders",
            await csv.Content
                .ReadAsStringAsync());
    }

    [Fact]
    public async Task Invalid_attachment_type_is_rejected()
    {
        using var dispatcher =
            await CreateAuthenticatedClientAsync(
                PostgreSqlDatabaseFixture
                    .NorthsideDispatcherEmail);

        using var form =
            new MultipartFormDataContent();

        using var file =
            new ByteArrayContent(
                "binary"u8.ToArray());

        file.Headers.ContentType =
            new MediaTypeHeaderValue(
                "application/octet-stream");

        form.Add(
            file,
            "file",
            "unsafe.exe");

        var response =
            await dispatcher.PostAsync(
                $"/api/work-orders/{NorthsideWorkOrderOne}/attachments",
                form);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private async Task<JsonDocument>
        UploadTextAsync(
            HttpClient client,
            Guid workOrderId,
            string fileName,
            string content)
    {
        using var form =
            CreateTextContent(
                fileName,
                content);

        var response =
            await client.PostAsync(
                $"/api/work-orders/{workOrderId}/attachments",
                form);

        response.EnsureSuccessStatusCode();

        return await ReadJsonAsync(
            response);
    }

    private static
        MultipartFormDataContent
        CreateTextContent(
            string fileName,
            string content)
    {
        var form =
            new MultipartFormDataContent();

        var file =
            new ByteArrayContent(
                Encoding.UTF8
                    .GetBytes(content));

        file.Headers.ContentType =
            new MediaTypeHeaderValue(
                "text/plain");

        form.Add(
            file,
            "file",
            fileName);

        return form;
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

        response
            .EnsureSuccessStatusCode();

        using var document =
            await ReadJsonAsync(
                response);

        var token =
            document.RootElement
                .GetProperty("accessToken")
                .GetString();

        Assert.False(
            string.IsNullOrWhiteSpace(
                token));

        client.DefaultRequestHeaders
            .Authorization =
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

        return JsonDocument.Parse(
            content);
    }
}
