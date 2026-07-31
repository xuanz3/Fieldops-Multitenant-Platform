using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FieldOps.Application.Auditing;

namespace FieldOps.Api.Auditing;

public sealed class HttpAuditActorContext
    : IAuditActorContext
{
    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public HttpAuditActorContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor
            .HttpContext?
            .User;

    public Guid? UserId
    {
        get
        {
            var value =
                Principal?.FindFirstValue(
                    JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(
                value,
                out var userId)
                    ? userId
                    : null;
        }
    }

    public string DisplayName =>
        Principal?.FindFirstValue(
            "name") ??
        Principal?.FindFirstValue(
            ClaimTypes.Name) ??
        "System";

    public string Role =>
        Principal?.FindFirstValue(
            ClaimTypes.Role) ??
        Principal?.FindFirstValue("role") ??
        "System";
}
