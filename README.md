# FieldOps Hub

[![Continuous Integration](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml/badge.svg)](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml)

FieldOps Hub is a portfolio-grade multi-tenant field service operations platform built with React, TypeScript, ASP.NET Core, Entity Framework Core and PostgreSQL.

> Status: Phase 7 complete. The product now covers Customer and WorkOrder management, role-owned field execution, controlled evidence, append-only audit history and operational reporting.

## Current capabilities

- multi-tenant PostgreSQL model
- Tenant-scoped Entity Framework query filters
- PBKDF2 password hashing and JWT authentication
- Tenant Admin, Dispatcher, Technician and Client roles
- Customer and WorkOrder REST APIs
- Dispatcher assignment and reassignment
- Technician start and completion submission
- Client approval and reopen workflow
- optimistic concurrency on WorkOrder writes
- controlled WorkOrder attachments
- PDF, PNG, JPEG and TXT allow-list
- 5 MB attachment limit
- SHA-256 attachment integrity metadata
- role- and ownership-authorised downloads
- automatic Customer, WorkOrder and attachment audit events
- per-Tenant SHA-256 audit chain
- PostgreSQL trigger blocking audit updates and deletes
- audit-chain verification
- operations report and CSV export
- responsive role-aware React workspace
- fictional two-Tenant demonstration data
- unit, PostgreSQL, HTTP API and frontend tests
- browser-to-real-API smoke validation
- GitHub Actions quality gates
- local-first, cloud-portable architecture

## Run the local demonstration

Pull the latest `main`, then on macOS double-click:

`START_LOCAL_DEMO.command`

Or run:

```bash
./scripts/start-local-demo.sh
```

Open `http://127.0.0.1:5173`.

All fictional demo accounts use:

- Tenant: `northside-property-services`
- Password: `FieldOps-Demo-2026!`

Role emails:

- Dispatcher: `dispatcher@northside.example.test`
- Technician: `technician@northside.example.test`
- Client: `client@northside.example.test`
- Tenant Admin: `admin@northside.example.test`

## Demonstration workflow

1. Sign in as Dispatcher and open **Dispatch**.
2. Assign a WorkOrder to the Technician.
3. Sign in as Technician, start work and submit a completion summary.
4. Open **Evidence** and upload a PDF, PNG, JPEG or TXT file.
5. Sign in as Client and approve or reopen the linked work.
6. Sign in as Dispatcher and inspect **Audit log** and **Reports**.
7. Export the operations report as CSV.

## Verified security behaviour

The automated suite proves:

- Tenant isolation
- signed Tenant identity
- role authorization
- header-spoofing resistance
- cross-Tenant relationship rejection
- assigned-Technician-only execution
- linked-Client-only decisions
- optimistic concurrency
- attachment type and size controls
- role-scoped evidence access
- SHA-256 file integrity metadata
- valid Tenant audit chains
- PostgreSQL rejection of direct audit mutation
- dependency and repository quality gates

## Architecture

- React and TypeScript browser client
- ASP.NET Core modular monolith on .NET 10 LTS
- PostgreSQL and Entity Framework Core
- Docker-based local services
- Vite local reverse proxy
- GitHub Actions continuous integration

Small demonstration attachments are stored in PostgreSQL for deterministic zero-cost local execution. Production-scale object storage and malware scanning are documented as future deployment concerns.

## Documentation

- [Phase 6 role-owned workflow](docs/phases/phase-06-role-workflow.md)
- [Phase 7 evidence, audit and reporting](docs/phases/phase-07-evidence-audit-reporting.md)
- [Controlled evidence decision](docs/decisions/ADR-010-controlled-evidence-storage.md)
- [Append-only audit decision](docs/decisions/ADR-011-append-only-audit-chain.md)
- [Phase 7 test coverage](docs/testing/phase-07-evidence-audit-report-tests.md)
- [v0.7.0 release notes](docs/releases/v0.7.0.md)
- [Final screenshot plan](docs/evidence/FINAL_SCREENSHOT_PLAN.md)

## Current limitations

Production object storage, malware scanning, external audit anchoring, notifications, MFA, refresh tokens, secure production cookie authentication and cloud deployment are not implemented yet. Phase 8 completes deployment, final evidence, README presentation and portfolio packaging.
