using FieldOps.Infrastructure.DependencyInjection;
using FieldOps.Infrastructure.Persistence;
using FieldOps.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddFieldOpsInfrastructure(builder.Configuration);

var app = builder.Build();

if (args.Contains("--seed-demo", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FieldOpsDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();

    await dbContext.Database.MigrateAsync();
    await seeder.SeedAsync();

    var tenantCount = await dbContext.Tenants.CountAsync();
    var customerCount = await dbContext.Customers
        .IgnoreQueryFilters()
        .CountAsync();
    var workOrderCount = await dbContext.WorkOrders
        .IgnoreQueryFilters()
        .CountAsync();

    Console.WriteLine(
        $"Demo data ready: tenants={tenantCount}, customers={customerCount}, workOrders={workOrderCount}");
    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapGet("/api/info", () => Results.Ok(new
{
    service = "FieldOps.Api",
    phase = 2,
    status = "data-foundation",
    timestamp = DateTimeOffset.UtcNow
}));

app.Run();

public partial class Program;
