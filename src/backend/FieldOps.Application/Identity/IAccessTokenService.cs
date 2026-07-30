using FieldOps.Domain.Identity;
using FieldOps.Domain.Tenants;

namespace FieldOps.Application.Identity;

public interface IAccessTokenService
{
    AccessTokenResult Create(
        UserAccount user,
        Tenant tenant);
}

public sealed record AccessTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAt);
