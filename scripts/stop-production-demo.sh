#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(
  cd "$(dirname "${BASH_SOURCE[0]}")/.."
  pwd
)"
ENV_FILE="$ROOT_DIR/.fieldops-runtime/production.env"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "No production runtime environment file exists."
  exit 0
fi

cd "$ROOT_DIR"

docker compose \
  --project-name fieldops-hub-production \
  --env-file "$ENV_FILE" \
  -f deploy/docker-compose.production.yml \
  down

echo "FieldOps Hub production deployment stopped."
