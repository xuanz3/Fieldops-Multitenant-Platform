# Phase 2 — Checkpoint 3 Review

## Delivered

- Remediated the vulnerable transitive Microsoft.OpenApi dependency
- Pinned local `dotnet-ef` tooling for reproducible migration commands
- Added the EF Core design-time context factory
- Created the first versioned PostgreSQL migration
- Added tenant-safe composite relationships and scoped uniqueness constraints
- Added an idempotent fictional demonstration-data seeder
- Added repeatable migration, seeding and local-reset scripts
- Applied and inspected the schema against local PostgreSQL

## Fictional demonstration organisations

- Northside Property Services
- Bayside Facility Group

The names, email addresses, customer records and work orders are invented. They contain no real personal or commercial data.

## Database protections introduced

- Tenant slug is globally unique
- Customer reference is unique within a tenant
- Work-order reference is unique within a tenant
- A work order references a customer through both `TenantId` and `CustomerId`
- Work-order version is configured for later optimistic-concurrency handling

## Important limitations

- Query filters and constraints have not yet been exercised by automated PostgreSQL integration tests
- Authentication and role authorization remain out of scope
- The API does not yet expose complete customer or work-order endpoints
- Azure and paid cloud resources are not used

## Cost and portability

Checkpoint 3 runs with local Docker and standard PostgreSQL at no monetary cost. Migration and seed commands use environment variables and are not tied to Azure.
