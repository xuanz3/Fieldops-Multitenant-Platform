using System.Text.Json.Serialization;
using FieldOps.Api.Auditing;
using FieldOps.Api.Authentication;
using FieldOps.Api.Authorization;
using FieldOps.Api.Endpoints;
using FieldOps.Application.Auditing;
using FieldOps.Application.Identity;
using FieldOps.Application.Tenancy;
using FieldOps.Infrastructure.DependencyInjection;
using FieldOps.Infrastructure.Persistence;
using FieldOps.Infrastructure.Persistence.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var seedDemoRequested =
    args.Contains(
        "--seed-demo",
        StringComparer.OrdinalIgnoreCase);

var builder =
    WebApplication.CreateBuilder(args);

var jwtOptions =
    JwtOptions.Create(
        builder.Configuration,
        allowEphemeralSigningKey:
            seedDemoRequested);

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureHttpJsonOptions(
    options =>
    {
        options.SerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddScoped<
    ITenantContext,
    HttpTenantContext>();

builder.Services.AddScoped<
    IAuditActorContext,
    HttpAuditActorContext>();

builder.Services.AddScoped<
    IAccessTokenService,
    JwtAccessTokenService>();

builder.Services.AddFieldOpsInfrastructure(
    builder.Configuration);

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters =
            jwtOptions
                .CreateValidationParameters();
    });

builder.Services.AddAuthorization(
    FieldOpsPolicies.Configure);

var app = builder.Build();

if (seedDemoRequested)
{
    await using var scope =
        app.Services.CreateAsyncScope();

    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<
                FieldOpsDbContext>();

    var seeder =
        scope.ServiceProvider
            .GetRequiredService<
                DemoDataSeeder>();

    var evidenceSeeder =
        scope.ServiceProvider
            .GetRequiredService<
                DemoEvidenceSeeder>();

    await dbContext.Database
        .MigrateAsync();

    await seeder.SeedAsync();
    await evidenceSeeder.SeedAsync();

    var tenantCount =
        await dbContext.Tenants
            .CountAsync();

    var customerCount =
        await dbContext.Customers
            .IgnoreQueryFilters()
            .CountAsync();

    var workOrderCount =
        await dbContext.WorkOrders
            .IgnoreQueryFilters()
            .CountAsync();

    var userCount =
        await dbContext.UserAccounts
            .IgnoreQueryFilters()
            .CountAsync();

    var attachmentCount =
        await dbContext
            .WorkOrderAttachments
            .IgnoreQueryFilters()
            .CountAsync();

    var auditEventCount =
        await dbContext.AuditEvents
            .IgnoreQueryFilters()
            .CountAsync();

    Console.WriteLine(
        $"Demo data ready: tenants={tenantCount}, customers={customerCount}, workOrders={workOrderCount}, users={userCount}, attachments={attachmentCount}, auditEvents={auditEventCount}");

    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapAuthenticationEndpoints();
app.MapAuthorisationEndpoints();
app.MapCustomerEndpoints();
app.MapWorkOrderEndpoints();
app.MapWorkflowEndpoints();
app.MapAttachmentEndpoints();
app.MapAuditEndpoints();
app.MapReportEndpoints();

app.MapGet(
    "/api/info",
    () => Results.Ok(new
    {
        service = "FieldOps.Api",
        phase = 7,
        status =
            "evidence-audit-reporting",
        timestamp =
            DateTimeOffset.UtcNow
    }));

app.Run();

public partial class Program;
