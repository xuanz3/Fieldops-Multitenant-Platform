using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FieldOps.Api.Authentication;

public sealed class JwtOptions
{
    private JwtOptions(
        string issuer,
        string audience,
        string signingKey,
        int accessTokenMinutes)
    {
        Issuer = issuer;
        Audience = audience;
        SigningKey = signingKey;
        AccessTokenMinutes = accessTokenMinutes;
    }

    public string Issuer { get; }

    public string Audience { get; }

    public string SigningKey { get; }

    public int AccessTokenMinutes { get; }

    public static JwtOptions Create(
        IConfiguration configuration,
        bool allowEphemeralSigningKey)
    {
        var issuer =
            configuration["Authentication:Jwt:Issuer"]
            ?? "FieldOps.Api";

        var audience =
            configuration["Authentication:Jwt:Audience"]
            ?? "FieldOps.Web";

        var signingKey =
            configuration["Authentication:Jwt:SigningKey"];

        if (string.IsNullOrWhiteSpace(signingKey) &&
            allowEphemeralSigningKey)
        {
            signingKey = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(48));
        }

        if (string.IsNullOrWhiteSpace(signingKey) ||
            signingKey.Length < 48)
        {
            throw new InvalidOperationException(
                "Authentication:Jwt:SigningKey must be supplied through configuration and contain at least 48 characters.");
        }

        var accessTokenMinutes =
            configuration.GetValue<int?>(
                "Authentication:Jwt:AccessTokenMinutes")
            ?? 30;

        if (accessTokenMinutes is < 5 or > 120)
        {
            throw new InvalidOperationException(
                "JWT access-token lifetime must be between 5 and 120 minutes.");
        }

        return new JwtOptions(
            issuer,
            audience,
            signingKey,
            accessTokenMinutes);
    }

    public TokenValidationParameters CreateValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(SigningKey)),
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = FieldOpsClaimTypes.Name,
            RoleClaimType = FieldOpsClaimTypes.Role
        };
    }
}
