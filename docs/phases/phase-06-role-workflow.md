# Phase 6 — Assignment, Technician Execution and Client Approval

## Outcome

Phase 6 completes the first end-to-end FieldOps business workflow:

`Dispatcher → Technician → Client → Completed or Reopened`

## Dispatcher workflow

- View active Technician and Client users
- Link a Client user to a Customer
- View Submitted, Assigned and Reopened work
- Assign or reassign a WorkOrder
- Preserve Tenant and role boundaries

## Technician workflow

- View only assigned WorkOrders
- Start Assigned work
- Submit an InProgress WorkOrder with a completion summary
- View records awaiting Client approval and completed history

## Client workflow

- View only WorkOrders belonging to linked Customer records
- Review Technician completion summaries
- Approve a WorkOrder as Completed
- Reopen a WorkOrder with a required reason

## Concurrency and security

Every workflow action carries the current WorkOrder Version. The domain and Entity Framework concurrency token reject stale updates. Composite Tenant relationships prevent cross-Tenant Client and Technician references.

## Demonstration data

The fictional Northside Tenant includes:

- One Dispatcher
- One Technician
- One Client
- Two Customers linked to the Client
- One InProgress WorkOrder
- One WorkOrder awaiting Client approval

## Scope exclusions

File attachments, immutable audit events, reporting, cloud deployment, refresh tokens and production cookie authentication remain later work.

## Cost

USD 0.
