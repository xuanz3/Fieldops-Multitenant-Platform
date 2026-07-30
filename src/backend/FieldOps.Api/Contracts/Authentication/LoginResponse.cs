namespace FieldOps.Api.Contracts.Authentication;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    AuthenticatedUserResponse User);

public sealed record AuthenticatedUserResponse(
    Guid UserId,
    Guid TenantId,
    string TenantSlug,
    string TenantName,
    string Email,
    string DisplayName,
    string Role);
