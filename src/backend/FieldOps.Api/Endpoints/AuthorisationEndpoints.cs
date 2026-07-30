using FieldOps.Api.Authorization;
using FieldOps.Application.Tenancy;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public static class AuthorisationEndpoints
{
    public static IEndpointRouteBuilder MapAuthorisationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/authorisation")
            .WithTags("Authorisation")
            .RequireAuthorization();

        group.MapGet(
            "/tenant-summary",
            TenantSummaryAsync);

        group.MapGet(
                "/admin",
                () => Results.Ok(new
                {
                    policy = FieldOpsPolicies.TenantAdminOnly
                }))
            .RequireAuthorization(
                FieldOpsPolicies.TenantAdminOnly);

        group.MapGet(
                "/dispatch",
                () => Results.Ok(new
                {
                    policy = FieldOpsPolicies.DispatchAccess
                }))
            .RequireAuthorization(
                FieldOpsPolicies.DispatchAccess);

        group.MapGet(
                "/technician",
                () => Results.Ok(new
                {
                    policy = FieldOpsPolicies.TechnicianAccess
                }))
            .RequireAuthorization(
                FieldOpsPolicies.TechnicianAccess);

        group.MapGet(
                "/client",
                () => Results.Ok(new
                {
                    policy = FieldOpsPolicies.ClientAccess
                }))
            .RequireAuthorization(
                FieldOpsPolicies.ClientAccess);

        return endpoints;
    }

    private static async Task<IResult> TenantSummaryAsync(
        ITenantContext tenantContext,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue)
        {
            return Results.Unauthorized();
        }

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existing =>
                    existing.Id == tenantContext.TenantId.Value,
                cancellationToken);

        if (tenant is null)
        {
            return Results.Unauthorized();
        }

        var customerCount =
            await dbContext.Customers.CountAsync(
                cancellationToken);

        var workOrderCount =
            await dbContext.WorkOrders.CountAsync(
                cancellationToken);

        return Results.Ok(new
        {
            tenantId = tenant.Id,
            tenantSlug = tenant.Slug,
            customerCount,
            workOrderCount
        });
    }
}
