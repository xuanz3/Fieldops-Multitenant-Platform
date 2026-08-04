# ADR-002: Shared PostgreSQL Database with Tenant-Scoped Rows

- Status: Accepted
- Date: 2026-07-30

## Context

FieldOps Hub needs realistic multi-tenant behaviour while remaining free to run locally, simple to deploy temporarily and portable between cloud platforms.

## Decision

Use one PostgreSQL database and shared tables. Tenant-owned rows contain `TenantId`. Entity Framework Core query filters provide default tenant scoping, while composite database constraints protect tenant-safe relationships.

## Why this option

- Works locally without paid infrastructure
- Uses standard PostgreSQL rather than one cloud vendor’s proprietary feature
- Keeps deployment and backup procedures simple for the current deployment model
- Still allows meaningful isolation tests and database constraints

## Consequences

Positive:

- Low operating cost and straightforward local setup
- One migration path and one schema to maintain
- Easy to move between compatible PostgreSQL providers

Negative:

- A filtering mistake can expose another tenant’s rows
- Administrative queries require extra care
- Very large tenants cannot be scaled independently without architectural change

## Alternatives considered

### Database per tenant

Provides stronger physical separation but creates excessive migration, connection and cost complexity for this project.

### Schema per tenant

Improves logical separation but complicates migrations and connection management without enough benefit for the current scale.

## Review trigger

Reconsider this decision if legal isolation requirements, independent tenant scaling or very large tenant counts become real product requirements.
