# ADR-006: Model Workflow Ownership by Role

## Status

Accepted.

## Context

Dispatchers, technicians and clients require different responsibilities within the same work-order lifecycle.

## Decision

Use explicit domain methods and signed user identities to enforce workflow ownership:

- Tenant Admin or Dispatcher links a Client user to a Customer.
- Tenant Admin or Dispatcher assigns a work order to an active Technician in the same tenant.
- Only the assigned Technician may start and submit that work order.
- Only the Client linked to the work order's Customer may approve or reopen it.
- Tenant Admin may inspect all role workflows for administration.

All workflow writes require the current work-order `Version`. Composite tenant foreign keys protect Customer-to-Client and WorkOrder-to-Technician relationships.

## Consequences

- A Technician cannot act on another Technician's assignment.
- A Client cannot view or decide work for an unrelated Customer.
- Reopened work returns to Dispatcher control and clears the previous assignment.
- Workflow rules are enforced in the domain, API and database layers.
