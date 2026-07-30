using System.Security.Claims;
using FieldOps.Api.Authentication;
using FieldOps.Api.Contracts.Authentication;
using FieldOps.Application.Identity;
using FieldOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost(
                "/login",
                LoginAsync)
            .AllowAnonymous();

        group.MapGet(
                "/me",
                MeAsync)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        FieldOpsDbContext dbContext,
        IPasswordHasher passwordHasher,
        IAccessTokenService accessTokenService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TenantSlug) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new
            {
                error = "Tenant slug, email and password are required."
            });
        }

        var tenantSlug =
            request.TenantSlug.Trim().ToLowerInvariant();

        var email =
            request.Email.Trim().ToLowerInvariant();

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existing => existing.Slug == tenantSlug,
                cancellationToken);

        if (tenant is null)
        {
            return Results.Unauthorized();
        }

        var user = await dbContext.UserAccounts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existing =>
                    existing.TenantId == tenant.Id &&
                    existing.Email == email,
                cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            return Results.Unauthorized();
        }

        var token = accessTokenService.Create(
            user,
            tenant);

        return Results.Ok(
            new LoginResponse(
                token.AccessToken,
                "Bearer",
                token.ExpiresAt,
                new AuthenticatedUserResponse(
                    user.Id,
                    tenant.Id,
                    tenant.Slug,
                    tenant.Name,
                    user.Email,
                    user.DisplayName,
                    user.Role.ToString())));
    }

    private static async Task<IResult> MeAsync(
        ClaimsPrincipal principal,
        FieldOpsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();

        if (!userId.HasValue)
        {
            return Results.Unauthorized();
        }

        var user = await dbContext.UserAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existing =>
                    existing.Id == userId.Value &&
                    existing.IsActive,
                cancellationToken);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .SingleOrDefaultAsync(
                existing =>
                    existing.Id == user.TenantId,
                cancellationToken);

        if (tenant is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(
            new AuthenticatedUserResponse(
                user.Id,
                tenant.Id,
                tenant.Slug,
                tenant.Name,
                user.Email,
                user.DisplayName,
                user.Role.ToString()));
    }
}
