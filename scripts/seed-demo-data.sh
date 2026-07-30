#!/bin/bash
set -Eeuo pipefail

export ConnectionStrings__FieldOps="${ConnectionStrings__FieldOps:-Host=localhost;Port=5432;Database=fieldops;Username=fieldops;Password=fieldops_dev_password}"

dotnet run \
  --project src/backend/FieldOps.Api/FieldOps.Api.csproj \
  -- --seed-demo
