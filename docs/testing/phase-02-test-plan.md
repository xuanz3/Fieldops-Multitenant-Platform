# Phase 2 Test Plan

## Unit tests

- Reject invalid tenant and customer names
- Preserve explicit TenantId ownership
- Preserve valid and invalid work-order state transitions

## PostgreSQL integration tests

- Tenant A queries cannot return Tenant B customers
- Tenant A queries cannot return Tenant B work orders
- Missing tenant context returns no tenant-scoped rows
- Cross-tenant customer/work-order relationship is rejected
- Duplicate work-order reference in the same tenant is rejected
- The same reference is allowed in two different tenants
- Schema creation and seed data are reproducible

## Evidence rules

- Tests must use fictional data only
- PostgreSQL must be the real test database, not an in-memory substitute
- Test results must identify the environment and command used
- Failed tests are fixed through additional commits rather than hidden
