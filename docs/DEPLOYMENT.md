# FieldOps Hub Deployment Guide

## Local development demonstration

On macOS, double-click:

`START_LOCAL_DEMO.command`

The launcher automatically selects free ports for PostgreSQL, ASP.NET Core and Vite. This prevents conflicts with other Docker projects or locally installed PostgreSQL services.

Stop the database with:

`STOP_LOCAL_DEMO.command`

## Production container demonstration

On macOS, double-click:

`START_PRODUCTION_DEMO.command`

The command:

1. creates a private runtime environment file
2. generates database and JWT secrets
3. selects a free host web port
4. builds the web and API images
5. starts PostgreSQL, API and Nginx
6. applies migrations and fictional seed data
7. waits for health checks
8. opens the application

Stop it with:

`STOP_PRODUCTION_DEMO.command`

## Manual Docker Compose deployment

Copy:

`deploy/.env.production.example`

to a secure environment file outside source control, then replace every placeholder.

Run:

```bash
docker compose \
  --env-file /secure/path/fieldops.env \
  -f deploy/docker-compose.production.yml \
  up -d --build
```

## Service boundary

- `web`: public Nginx entry point
- `api`: private ASP.NET Core service
- `postgres`: private PostgreSQL service

Only the web port is published.

## Additional production controls

A public deployment should additionally provide:

- HTTPS termination
- managed secret storage
- automated encrypted backups
- central logs and metrics
- object storage and malware scanning
- a managed PostgreSQL service or documented recovery process
- environment-specific scaling and availability controls
