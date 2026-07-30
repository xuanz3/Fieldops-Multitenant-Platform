using FieldOps.Domain.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FieldOps.Api.Authorization;

public static class FieldOpsPolicies
{
    public const string TenantAdminOnly =
        "tenant-admin-only";

    public const string DispatchAccess =
        "dispatch-access";

    public const string TechnicianAccess =
        "technician-access";

    public const string ClientAccess =
        "client-access";

    public static void Configure(
        AuthorizationOptions options)
    {
        options.AddPolicy(
            TenantAdminOnly,
            policy => policy.RequireRole(
                UserRole.TenantAdmin.ToString()));

        options.AddPolicy(
            DispatchAccess,
            policy => policy.RequireRole(
                UserRole.TenantAdmin.ToString(),
                UserRole.Dispatcher.ToString()));

        options.AddPolicy(
            TechnicianAccess,
            policy => policy.RequireRole(
                UserRole.TenantAdmin.ToString(),
                UserRole.Technician.ToString()));

        options.AddPolicy(
            ClientAccess,
            policy => policy.RequireRole(
                UserRole.TenantAdmin.ToString(),
                UserRole.Client.ToString()));
    }
}
