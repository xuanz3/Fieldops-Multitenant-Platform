# ADR-003: Derive Tenant Context from Signed Access Tokens

## Status

Accepted.

## Context

A caller must not be able to select another tenant by modifying a query string, request body or HTTP header.

## Decision

After login, the API issues a signed JWT containing the user ID, tenant ID, tenant slug, role, display name, email, issue time and expiry time.

Authenticated requests derive `ITenantContext` only from the validated `tenant_id` claim. Request headers and payload fields are not accepted as tenant-authority sources.

## Consequences

- Tenant identity is protected by signature, issuer, audience and lifetime validation.
- Entity Framework Core query filters receive the authenticated tenant automatically.
- Signing keys must remain outside source control and be rotated through the deployment environment.
