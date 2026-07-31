# ADR-009: Role-Owned Operational Workflow

## Status

Accepted in Phase 6.

## Decision

FieldOps models the operational workflow through explicit domain methods and signed user identities:

- Dispatcher or Tenant Admin links a Client user to a Customer.
- Dispatcher or Tenant Admin assigns a WorkOrder to an active Technician in the same Tenant.
- Only the assigned Technician may start and submit that WorkOrder.
- Only the Client linked to the WorkOrder's Customer may approve or reopen it.
- Tenant Admin may inspect and exercise all role workflows for administration and demonstration.

All workflow writes require the current WorkOrder `Version`. Stale writes return HTTP 409.

Composite Tenant foreign keys protect Customer-to-Client and WorkOrder-to-Technician relationships at the database level.

## Consequences

- Tenant headers and request bodies cannot choose another Tenant.
- A Technician cannot act on another Technician's assignment.
- A Client cannot view or decide work for an unrelated Customer.
- Reopened work returns to Dispatcher control and clears the previous Technician assignment.
- Workflow history currently remains on the WorkOrder record; a separate immutable audit event model is deferred to Phase 7.
