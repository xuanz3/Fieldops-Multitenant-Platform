# Phase 7 — Evidence, Audit and Reporting

## Outcome

Phase 7 turns the operational workflow into a more complete enterprise product with controlled evidence, tamper-evident history and management reporting.

## Evidence

- WorkOrder attachment upload, list and download
- 5 MB maximum
- PDF, PNG, JPEG and TXT allow-list
- file-name normalisation
- SHA-256 digest
- Tenant and role access checks
- fictional completion note in demo data

## Audit

- automatic Customer, WorkOrder and attachment event capture
- per-Tenant sequence
- previous-hash and event-hash chain
- verification API and frontend status
- PostgreSQL trigger preventing audit UPDATE and DELETE
- searchable, paginated audit page

## Reporting

- total, open and completed WorkOrders
- completion rate
- average completion hours
- status and priority distribution
- Technician workload
- Customer workload
- attachment and audit-event counts
- CSV export

## Frontend

New pages:

- Evidence
- Audit log
- Reports

The navigation remains role-aware. Evidence is available to all four roles within their authorised WorkOrder scope. Audit and Reports are restricted to Tenant Admin and Dispatcher.

## Security maintenance

React Router is upgraded from 6.30.4 to 7.18.2 and the entire frontend test and production-build suite is rerun.

## Cost

USD 0.
