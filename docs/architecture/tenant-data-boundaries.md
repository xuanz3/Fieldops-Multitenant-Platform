# Tenant Data Boundaries

## Tenant Identity

A tenant represents one organisation using FieldOps Hub. Multiple tenants share the application and PostgreSQL database, while business records remain isolated.

Authenticated requests derive tenant identity from the validated `tenant_id` JWT claim. Query strings, request bodies and custom headers are not accepted as tenant-authority sources.

## Tenant-Owned Records

The following records carry a non-null `TenantId`:

- User accounts
- Customers
- Work orders
- Work-order attachments
- Audit events

## Enforcement Layers

1. Domain entities require tenant ownership.
2. Authentication supplies a signed tenant identity.
3. API policies enforce role and ownership rules.
4. Entity Framework Core query filters apply tenant scoping.
5. Composite database relationships prevent cross-tenant references.
6. Tenant-scoped uniqueness constraints protect business identifiers.
7. Integration tests verify that cross-tenant reads and writes fail.

## Administrative Access

Tenant Admin permissions remain inside the authenticated tenant. Administrative access does not bypass the tenant boundary.

## Limitation

Query filters are one part of the boundary. Raw SQL, background processing and database administration must preserve the same tenant constraints.
