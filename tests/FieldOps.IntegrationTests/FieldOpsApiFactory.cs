using FieldOps.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FieldOps.IntegrationTests;

public sealed class FieldOpsApiFactory
    : WebApplicationFactory<Program>
{
    public const string Issuer =
        "FieldOps.IntegrationTests";

    public const string Audience =
        "FieldOps.IntegrationTests.Client";

    public const string SigningKey =
        "fieldops-integration-tests-signing-key-2026-change-only-for-tests";

    private readonly string _connectionString;

    public FieldOpsApiFactory(
        string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:FieldOps"] =
                            _connectionString,
                        ["Authentication:Jwt:Issuer"] =
                            Issuer,
                        ["Authentication:Jwt:Audience"] =
                            Audience,
                        ["Authentication:Jwt:SigningKey"] =
                            SigningKey,
                        ["Authentication:Jwt:AccessTokenMinutes"] =
                            "30"
                    });
            });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                IDbContextOptionsConfiguration<
                    FieldOpsDbContext>>();

            services.RemoveAll<
                DbContextOptions<
                    FieldOpsDbContext>>();

            services.RemoveAll<
                FieldOpsDbContext>();

            services.AddDbContext<FieldOpsDbContext>(
                options =>
                    options.UseNpgsql(
                        _connectionString));
        });
    }
}
