# Phase 3 — Authentication and Role-Based Authorisation

## Objective

Introduce tenant-aware identity before complete customer and work-order APIs are exposed.

## Delivered

- Tenant-scoped `UserAccount` domain entity
- Tenant-scoped unique email constraint
- Versioned PBKDF2-SHA256 password records
- JWT access-token issuance and validation
- Tenant identity derived from the signed token
- Tenant Admin, Dispatcher, Technician and Client roles
- Role-based ASP.NET Core policies
- Authenticated identity and tenant-summary endpoints
- PostgreSQL migration and fictional users
- Unit, database and real HTTP API tests
- CI execution of the full security test suite

## Endpoints

| Method | Endpoint | Access |
|---|---|---|
| POST | `/api/auth/login` | Anonymous |
| GET | `/api/auth/me` | Authenticated |
| GET | `/api/authorisation/tenant-summary` | Authenticated |
| GET | `/api/authorisation/admin` | Tenant Admin |
| GET | `/api/authorisation/dispatch` | Tenant Admin or Dispatcher |
| GET | `/api/authorisation/technician` | Tenant Admin or Technician |
| GET | `/api/authorisation/client` | Tenant Admin or Client |

## Authentication flow

1. The caller submits tenant slug, email and password.
2. The account lookup is explicitly scoped to that tenant.
3. The password is verified against a salted PBKDF2 record.
4. The API issues a short-lived signed JWT.
5. Later requests validate signature, issuer, audience and expiry.
6. `ITenantContext` reads the signed tenant claim.
7. EF Core query filters apply the tenant to normal business queries.

## Demo accounts

| Tenant | Email | Role |
|---|---|---|
| Northside Property Services | `admin@northside.example.test` | Tenant Admin |
| Northside Property Services | `dispatcher@northside.example.test` | Dispatcher |
| Northside Property Services | `technician@northside.example.test` | Technician |
| Northside Property Services | `client@northside.example.test` | Client |
| Bayside Facility Group | `admin@bayside.example.test` | Tenant Admin |

Demo password: `FieldOps-Demo-2026!`

These credentials are fictional and are intended only for disposable local demo data.

## Required configuration

```bash
export Authentication__Jwt__SigningKey='replace-with-at-least-48-random-characters'
export ConnectionStrings__FieldOps='Host=localhost;Port=5432;Database=fieldops;Username=fieldops;Password=fieldops_dev_password'
```

## Security tests

The suite proves valid and invalid login behaviour, cross-tenant login rejection, missing and malformed token rejection, 403 role enforcement, signed tenant identity, tenant-header spoofing resistance and tenant-filtered data.

## Current limitations

Refresh tokens, logout revocation, MFA, password-reset email, account lockout, rate limiting, external identity providers and a user-management UI are not included yet.
