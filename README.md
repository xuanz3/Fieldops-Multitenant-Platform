# FieldOps Hub

[![Continuous Integration](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml/badge.svg)](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml)

FieldOps Hub is a portfolio-grade multi-tenant field service operations platform covering customer records, work orders, dispatch, technician execution and client approval.

> Status: Phase 6 complete. The application now supports an end-to-end role-owned workflow from Dispatcher assignment through Technician completion and Client approval or reopen.

## Current capabilities

- Multi-tenant PostgreSQL data model
- Tenant-scoped Entity Framework query filters and relationship constraints
- PBKDF2 password hashing and JWT authentication
- Tenant Admin, Dispatcher, Technician and Client roles
- Customer and WorkOrder REST APIs
- Customer-to-Client ownership
- WorkOrder-to-Technician assignment
- Technician start and completion submission workflow
- Client approval and reopen workflow
- WorkOrder optimistic concurrency
- Responsive role-aware React workspace
- Dashboard, Customers, Work orders, Dispatch, My work and Client approvals pages
- Fictional two-Tenant demonstration data
- Unit, PostgreSQL, HTTP API and frontend tests
- Browser-to-real-API workflow smoke validation
- GitHub Actions quality gates
- Local-first, cloud-portable architecture

## Run the local demonstration

Pull the latest `main`, then on macOS double-click:

`START_LOCAL_DEMO.command`

Or run:

```bash
./scripts/start-local-demo.sh
```

Open `http://127.0.0.1:5173`.

All demo accounts use:

- Tenant: `northside-property-services`
- Password: `FieldOps-Demo-2026!`

Role emails:

- Dispatcher: `dispatcher@northside.example.test`
- Technician: `technician@northside.example.test`
- Client: `client@northside.example.test`
- Tenant Admin: `admin@northside.example.test`

## Demonstration workflow

1. Sign in as Dispatcher and open **Dispatch**.
2. Link a Client to a Customer and assign a WorkOrder to the Technician.
3. Sign out and sign in as Technician.
4. Start the task and submit a completion summary.
5. Sign out and sign in as Client.
6. Approve the work or reopen it with a reason.

## Verified security behaviour

The automated suite proves Tenant isolation, role authorization, signed Tenant identity, header-spoofing resistance, cross-Tenant relationship rejection, assigned-Technician-only execution, linked-Client-only decisions, optimistic concurrency and dependency quality gates.

## Architecture

- React and TypeScript browser client
- ASP.NET Core modular monolith on .NET 10 LTS
- PostgreSQL and Entity Framework Core
- Docker-based local services
- Vite local reverse proxy
- GitHub Actions continuous integration

## Documentation

- [Phase 4 Customer and WorkOrder APIs](docs/phases/phase-04-customer-workorder-rest-apis.md)
- [Phase 5 frontend business workspace](docs/phases/phase-05-frontend-business-workspace.md)
- [Phase 6 role-owned workflow](docs/phases/phase-06-role-workflow.md)
- [Role-owned workflow decision](docs/decisions/ADR-009-role-owned-operational-workflow.md)
- [Phase 6 workflow tests](docs/testing/phase-06-workflow-test-results.md)
- [v0.6.0 release notes](docs/releases/v0.6.0.md)
- [Final screenshot plan](docs/evidence/FINAL_SCREENSHOT_PLAN.md)

## Current limitations

Attachments, immutable audit events, reporting, notifications, refresh tokens, MFA, secure production cookie authentication and cloud deployment are not implemented yet.
