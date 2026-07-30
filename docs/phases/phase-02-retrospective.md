# Phase 2 Retrospective — Domain and Multi-Tenant Data Foundation

## 1. What was built

Phase 2 introduced the persistent multi-tenant data foundation for FieldOps Hub:

- Tenant, Customer and WorkOrder domain entities
- Tenant ownership on all business records
- Entity Framework Core persistence with PostgreSQL
- Versioned `InitialTenantSchema` migration
- Tenant-scoped unique customer and work-order references
- Composite tenant/customer foreign-key protection
- Fictional two-tenant demonstration data
- Automatic Entity Framework query filters
- Six real PostgreSQL isolation and negative tests
- A dedicated PostgreSQL 17 GitHub Actions job
- A transitive dependency vulnerability scan
- A repository policy limiting final portfolio images to 15

## 2. Why each output exists

### Domain entities

The entities express business rules before API controllers or user interfaces are added. This prevents tenant ownership and work-order state rules from being scattered across endpoints.

### PostgreSQL migration

The committed migration makes the database schema reproducible and reviewable. A clean environment can create the same tables, indexes and constraints.

### Query filters

Customer and WorkOrder queries automatically apply the current tenant boundary. A missing tenant context returns no business rows.

### Database constraints

Application filtering is not enough by itself. Composite keys and foreign keys prevent a WorkOrder from referencing a Customer owned by another tenant.

### Integration tests

The tests use PostgreSQL rather than an in-memory substitute. They prove that the migration, filters, uniqueness rules and relationships behave together.

### CI PostgreSQL service

GitHub Actions creates a temporary PostgreSQL 17 service so every pull request validates tenant isolation without relying on a developer's local database.

### Image policy

Only 15 final product images may remain in the repository. Phase and Checkpoint screenshots are not retained.

## 3. GitHub evidence

Phase 2 was delivered through focused pull requests:

- Design and tenant-boundary planning
- Domain entities and EF Core mappings
- Database migration and fictional seed data
- PostgreSQL tenant-isolation tests
- Repository-wide legacy image cleanup
- Phase 2 finalisation and release

Each implementation pull request passed backend, frontend, Docker and relevant PostgreSQL checks before merge.

## 4. New terms and tools

### Tenant

An organisation whose records must remain isolated from every other organisation using the same application.

### Tenant context

The tenant identity associated with the current request or operation.

### EF Core

The .NET data-access framework that maps C# entities to PostgreSQL tables and queries.

### DbContext

The EF Core object responsible for querying, tracking and saving database entities.

### Global query filter

A rule automatically added to normal EF Core queries. FieldOps uses it to restrict Customer and WorkOrder rows to the active tenant.

### Migration

A versioned description of a database schema change.

### Composite foreign key

A relationship that uses more than one column. FieldOps uses `TenantId` and `CustomerId` together so a work order cannot cross tenant boundaries.

### Unique constraint

A database rule rejecting duplicate values. Business references are unique inside one tenant but may be reused by different tenants.

### Integration test

A test that exercises multiple real components together. The Phase 2 suite uses EF Core, migrations and PostgreSQL.

### Ephemeral database

A temporary database created for one test run and removed afterwards.

### Negative test

A test proving that an invalid or forbidden action is rejected.

### Optimistic concurrency

A later workflow mechanism that detects when two users attempt to update different versions of the same record.

## 5. What works

- Domain validation for tenants, customers and work orders
- Work-order state transition rules
- PostgreSQL schema creation from a committed migration
- Reproducible fictional demonstration data
- Tenant-only Customer and WorkOrder queries
- Zero business rows when tenant context is missing
- Cross-tenant WorkOrder/Customer relationships rejected by PostgreSQL
- Duplicate references rejected inside one tenant
- Shared references accepted across different tenants
- Unit and PostgreSQL integration tests in CI
- Local-first operation without paid cloud resources
- Repository-wide 15-image limit

## 6. What does not work yet

- User authentication and token issuance
- Role-based authorisation
- Complete Customer and WorkOrder REST APIs
- Dispatcher and Technician assignment workflows
- Attachments and completion evidence
- Client approval and reopen actions
- Audit-log persistence
- Operational reporting
- Concurrency-conflict API responses
- Permanent cloud deployment

These are intentionally assigned to later phases.

## 7. Interview explanation

> Phase 2 established the database security boundary before building business APIs. Every Customer and WorkOrder is tenant-owned. EF Core automatically filters normal queries by tenant, while PostgreSQL composite constraints prevent cross-tenant relationships at the database level. I verified the design with six integration tests that create a temporary PostgreSQL database, apply the committed migration, exercise valid and invalid cases, and remove the database after the run. The same suite runs in GitHub Actions.

## 8. Next phase

Phase 3 will introduce authentication, tenant-aware user identity and role-based authorisation for Tenant Admin, Dispatcher, Technician and Client.
