namespace FieldOps.Api.Contracts.Authentication;

public sealed record LoginRequest(
    string TenantSlug,
    string Email,
    string Password);
