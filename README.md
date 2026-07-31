# FieldOps Hub

[![Continuous Integration](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml/badge.svg)](https://github.com/xuanz3/Fieldops-Multitenant-Platform/actions/workflows/ci.yml)

FieldOps Hub is a portfolio-grade multi-tenant field service operations platform for managing customers, work orders, technician assignments, attachments, approvals and audit records.

> Status: Phase 5 complete. The project now includes an authenticated browser workspace for tenant-safe Customer and WorkOrder operations.

## Current capabilities

- Tenant, Customer, WorkOrder and UserAccount domain entities
- PostgreSQL persistence and versioned migrations
- Tenant-scoped query filtering and relationship constraints
- PBKDF2 password hashing
- JWT access-token issuance and validation
- Signed tenant identity
- Tenant Admin, Dispatcher, Technician and Client roles
- Customer and WorkOrder REST APIs
- WorkOrder optimistic concurrency through Version
- React login and protected application routes
- Tenant dashboard
- Customer search, pagination, create and update UI
- WorkOrder search, filtering, create and update UI
- Responsive and accessible browser workspace
- Fictional two-tenant demonstration data
- Unit, PostgreSQL, HTTP API and frontend tests
- Browser-to-real-API smoke validation
- GitHub Actions quality gates
- Local-first, cloud-portable architecture

## Run the local demonstration

On macOS, double-click:

`START_LOCAL_DEMO.command`

Or run:

```bash
./scripts/start-local-demo.sh
```

Then open `http://127.0.0.1:5173`.

Fictional demo credentials:

- Tenant: `northside-property-services`
- Email: `dispatcher@northside.example.test`
- Password: `FieldOps-Demo-2026!`

## Verified security behaviour

The automated suite proves tenant-only database queries, cross-tenant relationship rejection, 401 authentication failures, 403 role failures, signed tenant identity, tenant-header spoofing resistance, cross-tenant business-record hiding, stale work-order conflict handling, session expiry behaviour, clean dependency scanning and the final 15-image policy.

## Architecture

- React and TypeScript browser client
- ASP.NET Core modular monolith on .NET 10 LTS
- PostgreSQL and Entity Framework Core
- Docker-based local services
- Vite local reverse proxy
- GitHub Actions continuous integration

## Development approach

Each Phase is delivered as one complete branch, one main pull request, full automated validation, documentation and a versioned release.

The repository does not retain process screenshots. A maximum of 15 final product images may be stored in `docs/evidence/final/`.

## Documentation

- [Project charter](docs/project-charter.md)
- [Phase 3 authentication and authorisation](docs/phases/phase-03-authentication-authorisation.md)
- [Phase 4 Customer and WorkOrder APIs](docs/phases/phase-04-customer-workorder-rest-apis.md)
- [Phase 5 frontend business workspace](docs/phases/phase-05-frontend-business-workspace.md)
- [Browser session and API proxy decision](docs/decisions/ADR-008-browser-session-and-api-proxy.md)
- [Phase 5 frontend tests](docs/testing/phase-05-frontend-test-results.md)
- [v0.5.0 release notes](docs/releases/v0.5.0.md)
- [Final screenshot plan](docs/evidence/FINAL_SCREENSHOT_PLAN.md)

## Current limitations

Technician assignment, Client ownership, workflow transition controls, attachments, audit persistence, reporting, refresh tokens, MFA, secure production cookie authentication and cloud deployment are not implemented yet.
