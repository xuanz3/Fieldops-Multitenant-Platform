using FieldOps.Application.Tenancy;
using FieldOps.Infrastructure.Persistence;
using FieldOps.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FieldOps.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFieldOpsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("FieldOps")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:FieldOps must be configured.");

        services.AddScoped<ITenantContext, EmptyTenantContext>();
        services.AddDbContext<FieldOpsDbContext>(
            options => options.UseNpgsql(connectionString));
        services.AddScoped<DemoDataSeeder>();

        return services;
    }
}
