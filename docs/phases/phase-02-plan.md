# Phase 2 — Domain and Multi-Tenant Data Foundation

## Objective

Introduce a persistent, tenant-aware data model for FieldOps Hub before authentication and full business workflows are implemented.

## Deliverables

- Tenant, Customer and expanded WorkOrder domain models
- PostgreSQL persistence through Entity Framework Core
- Versioned database migration
- Two fictional demonstration tenants and associated records
- Application-level tenant query filtering
- Database constraints for tenant-safe relationships
- PostgreSQL integration tests in local development and CI

## Acceptance criteria

1. Every customer and work order is owned by exactly one tenant.
2. Normal application queries only return rows for the active tenant.
3. Missing tenant context returns no tenant-scoped business rows.
4. A work order cannot reference a customer from another tenant.
5. Duplicate work-order references are rejected within one tenant but may exist in different tenants.
6. A clean environment can create the schema and fictional seed data reproducibly.
7. PostgreSQL integration tests pass locally and in GitHub Actions.

## Non-goals

- User login, token issuance and role authorization
- Complete customer and work-order REST endpoints
- File storage and notification delivery
- Azure deployment or any paid cloud resource

## Cost and portability constraints

Phase 2 must run locally at no cost using Docker and PostgreSQL. The application model must not depend on Azure-specific database features. Cloud deployment remains optional and replaceable.
