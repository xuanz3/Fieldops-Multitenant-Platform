# ADR-003 — Derive Tenant Context from a Signed Access Token

## Status

Accepted.

## Context

FieldOps Hub stores multiple organisations in one PostgreSQL database. A caller must not be able to select another tenant by changing a query string, request body or HTTP header.

## Decision

After successful login, the API signs a JWT containing the user ID, tenant ID, tenant slug, role, display name, email, issue time and expiry time.

Authenticated requests derive `ITenantContext` only from the validated `tenant_id` claim. The API does not accept a tenant-selection header as an authority source.

## Consequences

The tenant identity is protected by signature, issuer, audience and lifetime validation. EF Core query filters receive the authenticated tenant automatically. A compromised signing key could still produce trusted claims, so the key must remain outside source control.
