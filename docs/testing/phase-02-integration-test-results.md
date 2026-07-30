# Phase 2 PostgreSQL Integration Test Results

## Local command

```bash
FIELDOPS_TEST_CONNECTION="Host=localhost;Port=<selected-port>;Database=fieldops;Username=fieldops;Password=fieldops_dev_password" \
dotnet test tests/FieldOps.IntegrationTests/FieldOps.IntegrationTests.csproj \
  --configuration Release
```

## CI environment

GitHub Actions starts an ephemeral PostgreSQL 17 service and runs the same integration-test project.

## Expected result

- Total integration tests: 6
- Failed: 0
- Skipped: 0

## Data handling

All tenants, customers, email addresses and work orders are fictional test records.
