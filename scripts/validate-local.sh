#!/bin/bash
set -Eeuo pipefail

dotnet restore FieldOps.sln
dotnet build FieldOps.sln --configuration Release --no-restore
dotnet test FieldOps.sln --configuration Release --no-build
(
  cd src/frontend/fieldops-web
  npm ci
  npm run lint
  npm run build
)

if command -v docker >/dev/null 2>&1; then
  docker compose config >/dev/null
fi

echo "Local validation passed."
