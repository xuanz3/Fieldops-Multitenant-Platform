# Phase 7 Evidence, Audit and Report Test Results

Automated coverage includes:

- attachment SHA-256 and size metadata
- path-safe file names
- 5 MB domain limit
- allow-listed content type and extension
- Dispatcher upload, list and download
- linked Client download access
- Client upload rejection
- Tenant and WorkOrder ownership enforcement
- automatic audit creation
- chain sequence and previous-hash verification
- PostgreSQL microsecond timestamp normalisation before audit hashing
- PostgreSQL trigger rejection of direct audit mutation
- operations report JSON
- CSV report export
- multipart browser API client behaviour
- role-aware Evidence, Audit and Reports navigation
- all earlier authentication, tenant, Customer, WorkOrder and workflow tests
- browser-to-real-PostgreSQL evidence, audit and report smoke test
