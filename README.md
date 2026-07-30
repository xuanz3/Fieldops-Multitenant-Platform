# FieldOps Hub

[![Continuous Integration](https://github.com/xuanz3/fieldops-multitenant-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/xuanz3/fieldops-multitenant-platform/actions/workflows/ci.yml)

FieldOps Hub is a portfolio-grade multi-tenant field service operations platform for managing customers, work orders, technician assignments, attachments, approvals and audit records.

> Status: Phase 2 in progress. Tenant boundaries are designed, while the domain entities and PostgreSQL persistence layer are being introduced through reviewed checkpoints.

## Why this project exists

The project demonstrates enterprise product engineering rather than a basic CRUD demonstration. The target evidence includes tenant isolation, role-based workflows, controlled state transitions, concurrency handling, automated tests, observability and reproducible deployment.

## Planned architecture

- React and TypeScript browser client
- ASP.NET Core modular monolith on .NET 10 LTS
- PostgreSQL and Entity Framework Core
- Docker Compose local environment
- GitHub Actions quality gates

## Development approach

Each phase is delivered through several reviewable checkpoints. A checkpoint normally contains a focused issue set, a feature branch, automated validation and one pull request. Pull requests remain open until their scope and CI evidence have been reviewed.

## Deployment strategy

FieldOps Hub is designed to run locally at no cost using Docker. Cloud environments are optional, replaceable and deployed only when an online demonstration is required. Azure is the initial demonstration target, not a permanent application dependency.

## Documentation

- [Project charter](docs/project-charter.md)
- [Phase 2 plan](docs/phases/phase-02-plan.md)
- [Architecture](docs/architecture/)
- [Decision records](docs/decisions/)
- [Test strategy](docs/testing/test-strategy.md)
- [Local development](docs/operations/local-development.md)

## Current limitations

Authentication, role-based authorization and the complete work-order API are not implemented yet. Phase 2 is introducing tenant-aware PostgreSQL persistence through small, reviewed checkpoints.
