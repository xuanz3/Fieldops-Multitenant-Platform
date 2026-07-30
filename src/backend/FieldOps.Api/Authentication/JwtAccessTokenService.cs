using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FieldOps.Application.Identity;
using FieldOps.Domain.Identity;
using FieldOps.Domain.Tenants;
using Microsoft.IdentityModel.Tokens;

namespace FieldOps.Api.Authentication;

public sealed class JwtAccessTokenService
    : IAccessTokenService
{
    private readonly JwtOptions _options;

    public JwtAccessTokenService(
        JwtOptions options)
    {
        _options = options;
    }

    public AccessTokenResult Create(
        UserAccount user,
        Tenant tenant)
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var expiresAt = issuedAt.AddMinutes(
            _options.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),
            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                issuedAt.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new Claim(
                FieldOpsClaimTypes.TenantId,
                tenant.Id.ToString()),
            new Claim(
                FieldOpsClaimTypes.TenantSlug,
                tenant.Slug),
            new Claim(
                FieldOpsClaimTypes.Role,
                user.Role.ToString()),
            new Claim(
                FieldOpsClaimTypes.Name,
                user.DisplayName),
            new Claim(
                FieldOpsClaimTypes.Email,
                user.Email)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AccessTokenResult(
            new JwtSecurityTokenHandler()
                .WriteToken(token),
            expiresAt);
    }
}
