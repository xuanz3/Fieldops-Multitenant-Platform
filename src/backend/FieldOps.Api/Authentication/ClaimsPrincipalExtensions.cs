using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FieldOps.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(
        this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(
            JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(value, out var userId)
            ? userId
            : null;
    }
}
