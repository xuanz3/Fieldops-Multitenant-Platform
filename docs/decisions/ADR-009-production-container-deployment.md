# ADR-009: Package the System as Three Container Services

## Status

Accepted.

## Context

The system needs a repeatable deployment that preserves a clear boundary between the browser application, API and database.

## Decision

Package FieldOps Hub as:

- Nginx serving the compiled React application
- A private ASP.NET Core API service
- A private PostgreSQL 17 service

Only Nginx publishes a host port. It proxies `/api` and `/health` to the API. The API applies Entity Framework Core migrations and repeatable fictional seed data before starting. PostgreSQL uses a persistent volume.

Secrets are supplied through an external environment file and are not committed.

## Health Checks

- PostgreSQL uses `pg_isready`.
- ASP.NET Core exposes `/health`.
- Nginx exposes `/web-health`.
- Service startup order is gated by Docker health status.

## Consequences

- The same Compose configuration can run on a workstation or Docker-enabled Linux host.
- Service health, migration and network boundaries are reproducible.
- A public deployment still requires HTTPS, managed secrets, backups, observability and an environment-specific recovery plan.
