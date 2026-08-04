# Testing

## Unit tests

Unit tests cover domain validation, workflow transitions, password hashing, attachment rules and audit hashing.

## PostgreSQL integration tests

Integration tests apply the real migration history and verify:

- Tenant query isolation
- Tenant-scoped uniqueness
- Relationship constraints
- Authentication and authorisation
- Work-order workflow rules
- Attachment access
- Audit immutability and chain verification
- Reporting and CSV output

## Frontend tests

Frontend tests cover session handling, API clients, protected navigation, role-specific pages and reusable components.

## Continuous integration

Each pull request runs:

- .NET restore and Release build
- Unit tests
- PostgreSQL integration and API security tests
- Frontend install, lint, tests and production build
- npm and NuGet vulnerability scans
- Docker Compose validation
- Production image builds
- Documentation, link, naming and asset checks
