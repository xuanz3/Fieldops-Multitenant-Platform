#!/bin/bash
set -Eeuo pipefail

export ConnectionStrings__FieldOps="${ConnectionStrings__FieldOps:-Host=localhost;Port=5432;Database=fieldops;Username=fieldops;Password=fieldops_dev_password}"

dotnet tool restore
dotnet tool run dotnet-ef database update \
  --project src/backend/FieldOps.Infrastructure/FieldOps.Infrastructure.csproj \
  --startup-project src/backend/FieldOps.Api/FieldOps.Api.csproj \
  --context FieldOpsDbContext
