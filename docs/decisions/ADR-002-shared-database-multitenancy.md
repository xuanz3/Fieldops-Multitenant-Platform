# ADR-002: Use Shared Tables with Tenant-Scoped Rows

## Status

Accepted.

## Context

FieldOps Hub stores multiple organisations in PostgreSQL and must remain portable across local and hosted environments.

## Decision

Use one PostgreSQL database with shared tables. Tenant-owned rows contain `TenantId`. Entity Framework Core query filters provide default tenant scoping, while composite keys and foreign-key constraints protect tenant-safe relationships.

## Consequences

### Benefits

- One schema and migration path
- Straightforward backup and deployment procedures
- Compatibility with standard PostgreSQL providers
- Testable tenant boundaries at both application and database layers

### Trade-offs

- Query-filter mistakes can create data-exposure risk.
- Administrative queries require explicit handling.
- Independent per-tenant scaling would require an architectural change.

## Review Trigger

Reconsider this decision if contractual isolation, regulatory requirements or independent tenant scaling require physical separation.
