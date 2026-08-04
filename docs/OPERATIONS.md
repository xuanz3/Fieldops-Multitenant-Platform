# Operations

## Health checks

- PostgreSQL uses `pg_isready`.
- The API exposes `/health`.
- Nginx exposes `/web-health`.

## Database lifecycle

The startup scripts apply Entity Framework Core migrations before loading repeatable fictional demonstration data.

## Local runtime files

Generated environment files and logs are stored in `.fieldops-runtime/` and are excluded from source control.

## Start and stop commands

Local development:

- `START_LOCAL_DEMO.command`
- `STOP_LOCAL_DEMO.command`

Container deployment:

- `START_PRODUCTION_DEMO.command`
- `STOP_PRODUCTION_DEMO.command`

## Recovery

A public deployment should document database backup, restore, secret rotation and service recovery procedures for its hosting environment.
