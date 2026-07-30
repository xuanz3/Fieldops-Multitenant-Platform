#!/bin/bash
set -Eeuo pipefail

printf "This deletes the local FieldOps PostgreSQL volume and all local demo data.\n"
read -r -p "Type RESET to continue: " confirmation
[[ "$confirmation" == "RESET" ]] || exit 1

docker compose down -v
docker compose up -d postgres

for attempt in $(seq 1 30); do
  if docker compose exec -T postgres pg_isready -U fieldops -d fieldops >/dev/null 2>&1; then
    break
  fi
  sleep 2
done

./scripts/apply-migrations.sh
./scripts/seed-demo-data.sh
