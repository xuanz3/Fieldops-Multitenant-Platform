# ADR-009: Production Container Deployment

## Status

Accepted in Production release.

## Decision

FieldOps Hub is packaged as three production services:

- Nginx-hosted React application
- ASP.NET Core API
- PostgreSQL 17 database

The web service is the only host-exposed service. It serves the compiled React application and reverse-proxies `/api` and `/health` to the private API service.

The API applies Entity Framework migrations and fictional demonstration seed data before starting. The database uses a persistent Docker volume and is not exposed to the host in the production Compose configuration.

Deployment secrets are supplied through an external environment file and are never committed.

## Health model

- PostgreSQL uses `pg_isready`.
- ASP.NET Core exposes `/health`.
- Nginx exposes `/web-health`.
- Service startup order is gated by Docker health checks.

## Portability

`deploy/docker-compose.production.yml` runs on a local workstation or a Linux virtual machine with Docker Compose. This demonstrates a production-style deployment boundary without claiming that the portfolio instance is a permanently hosted public service.

## Consequences

The final portfolio can prove build, startup, migration, health and service isolation with no cloud charge. Public HTTPS hosting, managed secrets, backups, observability and managed PostgreSQL remain environment-specific production concerns.
