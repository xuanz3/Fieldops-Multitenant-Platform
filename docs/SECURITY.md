# Security

## Authentication

User passwords are stored as PBKDF2 records. Successful login produces a signed JWT containing the user, role and tenant identity.

## Authorisation

The API applies explicit policies for Tenant Admin, Dispatcher, Technician and Client operations. Tenant identity is derived from validated claims rather than request headers.

## Tenant isolation

Tenant boundaries are enforced through:

- Entity Framework Core query filters
- Tenant-aware database relationships
- Scoped uniqueness constraints
- API ownership checks
- Cross-tenant negative tests

## Attachments

Work-order attachments are restricted by file type and size. Each file receives SHA-256 integrity metadata, and downloads are subject to tenant, role and work-order access rules.

## Audit history

Audit events form a per-tenant hash chain. PostgreSQL rejects direct updates and deletes against the audit table.

## Dependency checks

Continuous integration scans direct and transitive NuGet packages and runs npm audit at the moderate-severity threshold.

## Production considerations

A public deployment should add managed secrets, HTTPS, malware scanning, centralised logging, encrypted backups, MFA and formal incident-response procedures.
