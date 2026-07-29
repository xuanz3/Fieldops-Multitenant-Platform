#!/bin/bash
set -Eeuo pipefail

if command -v docker >/dev/null 2>&1; then
  docker compose up -d postgres
else
  echo "Docker is not installed; PostgreSQL was not started."
fi

echo "Backend: dotnet run --project src/backend/FieldOps.Api"
echo "Frontend: cd src/frontend/fieldops-web && npm run dev"
