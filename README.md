# FieldOps Hub

[![Continuous Integration](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml/badge.svg)](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml)

FieldOps Hub is a portfolio-grade multi-tenant field service operations platform for managing customers, work orders, technician assignments, attachments, approvals and audit records.

> Status: Phase 2 complete. The project now has a tested PostgreSQL multi-tenant data foundation. Authentication and complete business APIs are scheduled for later phases.

## Current capabilities

- Validated Tenant, Customer and WorkOrder domain entities
- Controlled WorkOrder state transitions
- PostgreSQL persistence through Entity Framework Core
- Versioned database migrations
- Tenant-scoped query filtering
- Database-enforced tenant/customer relationship boundaries
- Fictional two-tenant demonstration data
- Unit tests and real PostgreSQL integration tests
- GitHub Actions quality gates
- Local-first, cloud-portable architecture

## Architecture

- React and TypeScript browser client
- ASP.NET Core modular monolith on .NET 10 LTS
- PostgreSQL and Entity Framework Core
- Docker-based local services
- GitHub Actions continuous integration

## Verified multi-tenant behaviour

The PostgreSQL integration suite proves that:

1. A tenant reads only its own Customer and WorkOrder rows.
2. Missing tenant context returns no tenant-owned business rows.
3. A tenant cannot fetch another tenant's known work order.
4. Different tenants may use the same business reference.
5. Duplicate references inside one tenant are rejected.
6. Cross-tenant Customer and WorkOrder relationships are rejected.

## Development approach

Each phase is delivered through focused issues, branches, automated validation and reviewable pull requests.

The repository retains no phase-by-phase screenshot archive. A maximum of 15 final portfolio images may be stored in `docs/evidence/final/`.

## Deployment strategy

FieldOps Hub runs locally without paid cloud infrastructure. Cloud environments are optional, replaceable and deployed only when an online demonstration is required. Azure is the initial demonstration target rather than a permanent application dependency.

## Documentation

- [Project charter](docs/project-charter.md)
- [Phase 2 plan](docs/phases/phase-02-plan.md)
- [Phase 2 retrospective](docs/phases/phase-02-retrospective.md)
- [v0.2.0 release notes](docs/releases/v0.2.0.md)
- [Architecture](docs/architecture/)
- [Decision records](docs/decisions/)
- [Test strategy](docs/testing/test-strategy.md)
- [PostgreSQL integration results](docs/testing/phase-02-integration-test-results.md)
- [Local development](docs/operations/local-development.md)
- [Final screenshot plan](docs/evidence/FINAL_SCREENSHOT_PLAN.md)

## Current limitations

Authentication, role-based authorisation, complete Customer and WorkOrder APIs, attachments, audit persistence, reporting and cloud deployment are not implemented yet.
