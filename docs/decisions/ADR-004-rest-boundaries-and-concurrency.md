# ADR-004: Tenant-Safe REST Boundaries and Work Order Concurrency

## Status

Accepted in Customer and work-order API.

## Decision

Customer and WorkOrder endpoints use the authenticated JWT tenant claim through `ITenantContext`. Entity Framework query filters remain the authoritative data boundary.

Customer and WorkOrder management is limited to Tenant Admin and Dispatcher roles during this Phase. Technician assignment and Client ownership rules are deferred until the workflow model exists.

WorkOrder updates require the caller's current `Version`. A stale version returns HTTP 409 and Entity Framework also treats the column as a concurrency token.

## Consequences

- Caller-supplied tenant headers cannot select another tenant.
- Cross-tenant identifiers behave as not found.
- Duplicate tenant-scoped references return conflict.
- Concurrent edits cannot silently overwrite a newer work order.
- Delete endpoints are intentionally absent because operational history must be preserved.
