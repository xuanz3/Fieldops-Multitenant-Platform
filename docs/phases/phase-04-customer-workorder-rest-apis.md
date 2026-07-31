# Phase 4 — Customer and WorkOrder REST APIs

## Outcome

Phase 4 converts the tenant, customer and work-order model into authenticated business APIs.

## Customer API

- `GET /api/customers`
- `GET /api/customers/{id}`
- `POST /api/customers`
- `PUT /api/customers/{id}`
- Search, pagination and tenant-scoped reference uniqueness
- Tenant Admin and Dispatcher access

## WorkOrder API

- `GET /api/work-orders`
- `GET /api/work-orders/{id}`
- `POST /api/work-orders`
- `PUT /api/work-orders/{id}`
- Search, status, priority and customer filters
- Pagination and customer display data
- Tenant-safe customer relationship validation
- Version-based optimistic concurrency
- Tenant Admin and Dispatcher access

## HTTP behaviour

- 400: malformed or invalid input
- 401: missing or invalid authentication
- 403: authenticated role lacks access
- 404: record is absent from the signed tenant
- 409: duplicate reference or stale work-order version

## Security boundary

Tenant identity is not accepted from the URL, request body or `X-Tenant-Id`. It remains derived from the validated JWT claim introduced in Phase 3.

## Scope exclusions

Technician assignment, client-to-customer ownership, status transition commands, attachments, audit records and reporting remain later Phases.

## Cost

USD 0. All implementation and validation run locally and in GitHub Actions.
