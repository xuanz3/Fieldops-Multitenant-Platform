# FieldOps Hub

[![Continuous Integration](https://github.com/xuanz3/fieldops-multitenant-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/xuanz3/fieldops-multitenant-platform/actions/workflows/ci.yml)

FieldOps Hub is a portfolio-grade multi-tenant field service operations platform for managing customers, work orders, technician assignments, attachments, approvals and audit records.

> Status: Phase 2 in progress. Tenant data boundaries and the persistent domain model are being introduced through reviewed checkpoints.

## Why this project exists

The project demonstrates enterprise product engineering rather than a basic CRUD demonstration. The target evidence includes tenant isolation, role-based workflows, controlled state transitions, concurrency handling, automated tests, observability and reproducible deployment.

## Planned architecture

- React and TypeScript browser client
- ASP.NET Core modular monolith on .NET 10 LTS
- PostgreSQL and Entity Framework Core
- Docker Compose local environment
- GitHub Actions quality gates

## Documentation

- [Project charter](docs/project-charter.md)
- [Phase 2 plan](docs/phases/phase-02-plan.md)
- [Architecture](docs/architecture/)
- [Decision records](docs/decisions/)
- [Test strategy](docs/testing/test-strategy.md)
- [Local development](docs/operations/local-development.md)

## Current limitations

Authentication, tenant persistence and the full work order workflow are intentionally scheduled for later phases. Phase 1 proves project governance, architecture foundations and repeatable validation.
