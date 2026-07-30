# Phase 2 — Checkpoint 4 Review

## Delivered

- A separate PostgreSQL integration-test project
- Migration-based temporary test databases
- Tenant-specific Entity Framework contexts
- Automated negative tests for isolation and relationship boundaries
- A dedicated PostgreSQL service job in GitHub Actions
- Local and CI execution against PostgreSQL 17

## Automated evidence

The integration suite proves:

1. Tenant queries return only records owned by the active tenant.
2. Missing tenant context returns no customer or work-order rows.
3. Tenant A cannot fetch a known Tenant B work order.
4. The same business reference may exist in different tenants.
5. A duplicate reference inside one tenant is rejected.
6. A work order cannot reference another tenant's customer.

## Test lifecycle

Each run creates a uniquely named PostgreSQL database, applies the committed EF Core migration, inserts fictional records, executes the tests and drops the temporary database.

This prevents tests from depending on manually prepared local data.

## Important limitations

- Authentication and role authorization are not implemented yet.
- Administrative SQL can bypass application query filters.
- Full customer and work-order APIs remain out of scope.
- Concurrency conflict behaviour is tested in a later phase.
- No Azure or paid cloud resource is used.

## Cost and portability

The tests use standard PostgreSQL locally and in GitHub Actions. No Azure-specific feature is required.
