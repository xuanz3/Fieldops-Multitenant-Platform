using FieldOps.Domain.Customers;
using FieldOps.Domain.Identity;
using FieldOps.Domain.Tenants;
using FieldOps.Domain.WorkOrders;
using FieldOps.Infrastructure.Identity;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FieldOps.IntegrationTests;

public sealed class PostgreSqlDatabaseFixture
    : IAsyncLifetime
{
    public const string TestPassword =
        "FieldOps-Test-2026!";

    public const string NorthsideTenantSlug =
        "northside-property-services";

    public const string BaysideTenantSlug =
        "bayside-facility-group";

    public const string NorthsideAdminEmail =
        "admin@northside.example.test";

    public const string NorthsideDispatcherEmail =
        "dispatcher@northside.example.test";

    public const string NorthsideTechnicianEmail =
        "technician@northside.example.test";

    public const string NorthsideClientEmail =
        "client@northside.example.test";

    public const string BaysideAdminEmail =
        "admin@bayside.example.test";

    public static readonly Guid NorthsideTenantId =
        Guid.Parse(
            "11111111-1111-1111-1111-111111111111");

    public static readonly Guid BaysideTenantId =
        Guid.Parse(
            "22222222-2222-2222-2222-222222222222");

    public static readonly Guid NorthsideCustomerId =
        Guid.Parse(
            "11111111-1111-1111-1111-111111110001");

    public static readonly Guid BaysideCustomerId =
        Guid.Parse(
            "22222222-2222-2222-2222-222222220001");

    public static readonly Guid BaysideWorkOrderId =
        Guid.Parse(
            "22222222-2222-2222-2222-222222220101");

    public static readonly Guid
        NorthsideTechnicianUserId =
            Guid.Parse(
                "11111111-1111-1111-1111-111111130003");

    public static readonly Guid
        NorthsideClientUserId =
            Guid.Parse(
                "11111111-1111-1111-1111-111111130004");

    private string? _adminConnectionString;
    private string? _databaseName;

    public string ConnectionString { get; private set; } =
        string.Empty;

    public async Task InitializeAsync()
    {
        var baseConnectionString =
            Environment.GetEnvironmentVariable(
                "FIELDOPS_TEST_CONNECTION");

        if (string.IsNullOrWhiteSpace(
                baseConnectionString))
        {
            throw new InvalidOperationException(
                "FIELDOPS_TEST_CONNECTION must point to a real PostgreSQL service.");
        }

        var baseBuilder =
            new NpgsqlConnectionStringBuilder(
                baseConnectionString);

        _databaseName =
            $"fieldops_it_{Guid.NewGuid():N}";

        var adminBuilder =
            new NpgsqlConnectionStringBuilder(
                baseConnectionString)
            {
                Database = baseBuilder.Database,
                Pooling = false
            };

        _adminConnectionString =
            adminBuilder.ConnectionString;

        await using (var adminConnection =
                     new NpgsqlConnection(
                         _adminConnectionString))
        {
            await adminConnection.OpenAsync();

            await using var createCommand =
                new NpgsqlCommand(
                    $"CREATE DATABASE \"{_databaseName}\"",
                    adminConnection);

            await createCommand.ExecuteNonQueryAsync();
        }

        var testBuilder =
            new NpgsqlConnectionStringBuilder(
                baseConnectionString)
            {
                Database = _databaseName,
                Pooling = false
            };

        ConnectionString =
            testBuilder.ConnectionString;

        await using var dbContext =
            CreateDbContext(null);

        await dbContext.Database.MigrateAsync();
        await SeedAsync(dbContext);
    }

    public FieldOpsDbContext CreateDbContext(
        Guid? tenantId)
    {
        var options =
            new DbContextOptionsBuilder<FieldOpsDbContext>()
                .UseNpgsql(ConnectionString)
                .EnableDetailedErrors()
                .Options;

        return new FieldOpsDbContext(
            options,
            new TestTenantContext(tenantId));
    }

    public async Task DisposeAsync()
    {
        if (string.IsNullOrWhiteSpace(
                _adminConnectionString) ||
            string.IsNullOrWhiteSpace(
                _databaseName))
        {
            return;
        }

        NpgsqlConnection.ClearAllPools();

        await using var adminConnection =
            new NpgsqlConnection(
                _adminConnectionString);

        await adminConnection.OpenAsync();

        await using (var terminateCommand =
                     new NpgsqlCommand(
                         """
                         SELECT pg_terminate_backend(pid)
                         FROM pg_stat_activity
                         WHERE datname = @databaseName
                           AND pid <> pg_backend_pid();
                         """,
                         adminConnection))
        {
            terminateCommand.Parameters.AddWithValue(
                "databaseName",
                _databaseName);

            await terminateCommand
                .ExecuteNonQueryAsync();
        }

        await using var dropCommand =
            new NpgsqlCommand(
                $"DROP DATABASE IF EXISTS \"{_databaseName}\"",
                adminConnection);

        await dropCommand.ExecuteNonQueryAsync();
    }

    private static async Task SeedAsync(
        FieldOpsDbContext dbContext)
    {
        var timestamp =
            new DateTimeOffset(
                2026,
                7,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        dbContext.Tenants.AddRange(
            new Tenant(
                "Northside Property Services",
                NorthsideTenantSlug,
                NorthsideTenantId,
                timestamp),
            new Tenant(
                "Bayside Facility Group",
                BaysideTenantSlug,
                BaysideTenantId,
                timestamp));

        await dbContext.SaveChangesAsync();

        var passwordHasher =
            new Pbkdf2PasswordHasher();

        dbContext.UserAccounts.AddRange(
            new UserAccount(
                NorthsideTenantId,
                NorthsideAdminEmail,
                "Northside Admin",
                passwordHasher.Hash(TestPassword),
                UserRole.TenantAdmin,
                Guid.Parse(
                    "11111111-1111-1111-1111-111111130001"),
                timestamp),
            new UserAccount(
                NorthsideTenantId,
                NorthsideDispatcherEmail,
                "Northside Dispatcher",
                passwordHasher.Hash(TestPassword),
                UserRole.Dispatcher,
                Guid.Parse(
                    "11111111-1111-1111-1111-111111130002"),
                timestamp),
            new UserAccount(
                NorthsideTenantId,
                NorthsideTechnicianEmail,
                "Northside Technician",
                passwordHasher.Hash(TestPassword),
                UserRole.Technician,
                NorthsideTechnicianUserId,
                timestamp),
            new UserAccount(
                NorthsideTenantId,
                NorthsideClientEmail,
                "Northside Client",
                passwordHasher.Hash(TestPassword),
                UserRole.Client,
                NorthsideClientUserId,
                timestamp),
            new UserAccount(
                BaysideTenantId,
                BaysideAdminEmail,
                "Bayside Admin",
                passwordHasher.Hash(TestPassword),
                UserRole.TenantAdmin,
                Guid.Parse(
                    "22222222-2222-2222-2222-222222230001"),
                timestamp));

        await dbContext.SaveChangesAsync();

        dbContext.Customers.AddRange(
            new Customer(
                NorthsideTenantId,
                "CLIENT-001",
                "Northside Demo Client",
                "northside@example.test",
                NorthsideCustomerId,
                timestamp,
                NorthsideClientUserId),
            new Customer(
                NorthsideTenantId,
                "CLIENT-002",
                "Northside Retail Demo",
                "northside-retail@example.test",
                Guid.Parse(
                    "11111111-1111-1111-1111-111111110002"),
                timestamp),
            new Customer(
                BaysideTenantId,
                "CLIENT-001",
                "Bayside Demo Client",
                "bayside@example.test",
                BaysideCustomerId,
                timestamp));

        await dbContext.SaveChangesAsync();

        dbContext.WorkOrders.AddRange(
            new WorkOrder(
                NorthsideTenantId,
                NorthsideCustomerId,
                "WO-1001",
                "Inspect leaking kitchen tap",
                "Fictional test record.",
                WorkOrderPriority.Normal,
                Guid.Parse(
                    "11111111-1111-1111-1111-111111120001"),
                timestamp),
            new WorkOrder(
                NorthsideTenantId,
                Guid.Parse(
                    "11111111-1111-1111-1111-111111110002"),
                "WO-1002",
                "Replace damaged access panel",
                "Fictional test record.",
                WorkOrderPriority.High,
                Guid.Parse(
                    "11111111-1111-1111-1111-111111120002"),
                timestamp),
            new WorkOrder(
                BaysideTenantId,
                BaysideCustomerId,
                "WO-1001",
                "Test emergency lighting",
                "Fictional test record.",
                WorkOrderPriority.Urgent,
                BaysideWorkOrderId,
                timestamp));

        await dbContext.SaveChangesAsync();
    }
}
