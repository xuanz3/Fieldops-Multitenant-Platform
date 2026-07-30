using System.Security.Claims;
using FieldOps.Application.Tenancy;

namespace FieldOps.Api.Authentication;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var value = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirstValue(FieldOpsClaimTypes.TenantId);

            return Guid.TryParse(value, out var tenantId)
                ? tenantId
                : null;
        }
    }
}
