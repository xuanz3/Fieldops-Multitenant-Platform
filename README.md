# FieldOps Hub

[![Continuous Integration](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml/badge.svg)](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml)

FieldOps Hub is a portfolio-grade multi-tenant field service operations platform for managing customers, work orders, technician assignments, attachments, approvals and audit records.

> Status: Phase 4 complete. The backend now exposes tenant-safe Customer and WorkOrder REST APIs on top of tested PostgreSQL isolation, JWT authentication and role authorisation.

## Current capabilities

- Tenant, Customer, WorkOrder and UserAccount domain entities
- PostgreSQL persistence and versioned migrations
- Tenant-scoped query filtering and relationship constraints
- PBKDF2 password hashing
- JWT access-token issuance and validation
- Signed tenant identity
- Tenant Admin, Dispatcher, Technician and Client roles
- Customer list, search, pagination, detail, create and update APIs
- WorkOrder list, filter, pagination, detail, create and update APIs
- WorkOrder optimistic concurrency through Version
- Fictional two-tenant demonstration data
- Unit, PostgreSQL and real HTTP API tests
- GitHub Actions quality gates
- Local-first, cloud-portable architecture

## Verified security behaviour

The automated suite proves tenant-only database queries, cross-tenant relationship rejection, 401 authentication failures, 403 role failures, signed tenant identity, tenant-header spoofing resistance, cross-tenant business-record hiding, stale work-order conflict handling, clean dependency scanning and the final 15-image policy.

## Architecture

- React and TypeScript browser client
- ASP.NET Core modular monolith on .NET 10 LTS
- PostgreSQL and Entity Framework Core
- Docker-based local services
- GitHub Actions continuous integration

## Development approach

Each Phase is delivered as one complete branch, one main pull request, full automated validation, documentation and a versioned release.

The repository does not retain process screenshots. A maximum of 15 final product images may be stored in `docs/evidence/final/`.

## Documentation

- [Project charter](docs/project-charter.md)
- [Phase 2 retrospective](docs/phases/phase-02-retrospective.md)
- [Phase 3 authentication and authorisation](docs/phases/phase-03-authentication-authorisation.md)
- [Phase 4 Customer and WorkOrder APIs](docs/phases/phase-04-customer-workorder-rest-apis.md)
- [Token-derived tenant decision](docs/decisions/ADR-006-token-derived-tenant-context.md)
- [REST boundary and concurrency decision](docs/decisions/ADR-007-rest-boundaries-and-concurrency.md)
- [Phase 4 API tests](docs/testing/phase-04-api-test-results.md)
- [v0.4.0 release notes](docs/releases/v0.4.0.md)
- [Final screenshot plan](docs/evidence/FINAL_SCREENSHOT_PLAN.md)

## Current limitations

The production frontend, technician assignment, Client ownership, workflow transition endpoints, attachments, audit persistence, reporting, refresh tokens, MFA and cloud deployment are not implemented yet.
