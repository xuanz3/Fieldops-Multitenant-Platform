# ADR-004: Enforce Tenant-Safe REST Boundaries and Optimistic Concurrency

## Status

Accepted.

## Context

Customer and work-order APIs must prevent cross-tenant access and avoid silent overwrites when two users edit the same work order.

## Decision

Customer and work-order endpoints use the authenticated tenant context. Entity Framework Core query filters remain the default data boundary, and endpoint policies limit management actions to authorised roles.

Work-order updates require the caller's current `Version`. A stale version returns HTTP 409, and Entity Framework Core treats the version column as a concurrency token.

## Consequences

- Caller-supplied tenant values cannot select another tenant.
- Cross-tenant identifiers behave as not found.
- Duplicate tenant-scoped references return conflict.
- Concurrent edits cannot silently overwrite newer data.
- Work-order deletion is intentionally absent to preserve operational history.
