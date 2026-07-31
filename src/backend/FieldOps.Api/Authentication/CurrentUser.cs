using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FieldOps.Api.Authentication;

internal static class CurrentUser
{
    public static Guid RequireUserId(
        ClaimsPrincipal principal)
    {
        var value =
            principal.FindFirstValue(
                JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new InvalidOperationException(
                "The authenticated token does not contain a valid user identifier.");
        }

        return userId;
    }
}
