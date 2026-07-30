# Tenant Data Boundaries

## Meaning of a tenant

A tenant represents one customer organisation using FieldOps Hub. Two tenants share the same application and PostgreSQL server, but their business records must remain isolated.

## Tenant-scoped data

The following records must carry a non-null `TenantId`:

- Customer
- WorkOrder
- Future assignment, attachment, notification and audit records

## Access rule

A request may only read or modify records whose `TenantId` matches the trusted tenant context selected by the backend. The browser must never be trusted to choose an arbitrary tenant identifier.

## Planned enforcement layers

1. Domain ownership: tenant-scoped entities require `TenantId`.
2. Application query filtering: Entity Framework Core automatically adds the active tenant condition.
3. Database relationships: composite keys prevent a work order from referencing another tenant’s customer.
4. Uniqueness rules: business references are unique inside one tenant, not globally.
5. Automated negative tests: cross-tenant reads and writes must fail.

## Important limitation

An Entity Framework query filter is a safety layer, not complete security by itself. Raw SQL, administrative tools or incorrectly configured background jobs could bypass it. Database constraints and later authorization checks provide additional defence.
