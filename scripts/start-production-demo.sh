#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(
  cd "$(dirname "${BASH_SOURCE[0]}")/.."
  pwd
)"
RUNTIME_DIR="$ROOT_DIR/.fieldops-runtime"
ENV_FILE="$RUNTIME_DIR/production.env"
PROJECT_NAME="fieldops-hub-production"

mkdir -p "$RUNTIME_DIR"

for tool in docker curl python3; do
  if ! command -v "$tool" >/dev/null 2>&1; then
    echo "Missing required tool: $tool"
    exit 1
  fi
done

if ! docker info >/dev/null 2>&1; then
  if [[ "$(uname -s)" == "Darwin" ]]; then
    open -a Docker
  fi

  for _ in {1..60}; do
    if docker info >/dev/null 2>&1; then
      break
    fi
    sleep 5
  done
fi

choose_port() {
  python3 <<'PY'
import socket

for port in range(8088, 8288):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        try:
            sock.bind(("127.0.0.1", port))
        except OSError:
            continue
        print(port)
        raise SystemExit(0)

raise SystemExit("No free deployment port was found.")
PY
}

WEB_PORT="$(choose_port)"

python3 - "$ENV_FILE" "$WEB_PORT" <<'PY'
from pathlib import Path
import secrets
import sys

path = Path(sys.argv[1])
web_port = sys.argv[2]

if path.exists():
    lines = {}
    for raw in path.read_text(encoding="utf-8").splitlines():
        if "=" in raw:
            key, value = raw.split("=", 1)
            lines[key] = value
else:
    lines = {}

lines.setdefault(
    "POSTGRES_PASSWORD",
    secrets.token_urlsafe(32),
)
lines.setdefault(
    "JWT_SIGNING_KEY",
    secrets.token_urlsafe(64),
)
lines["POSTGRES_DB"] = "fieldops"
lines["POSTGRES_USER"] = "fieldops"
lines["JWT_ISSUER"] = "FieldOps.ProductionDemo"
lines["JWT_AUDIENCE"] = "FieldOps.ProductionDemo.Client"
lines["FIELDOPS_WEB_PORT"] = web_port

ordered = [
    "POSTGRES_DB",
    "POSTGRES_USER",
    "POSTGRES_PASSWORD",
    "JWT_ISSUER",
    "JWT_AUDIENCE",
    "JWT_SIGNING_KEY",
    "FIELDOPS_WEB_PORT",
]

path.write_text(
    "\n".join(
        f"{key}={lines[key]}"
        for key in ordered
    ) + "\n",
    encoding="utf-8",
)
PY

cd "$ROOT_DIR"

echo "Building and starting the production container deployment..."
docker compose \
  --project-name "$PROJECT_NAME" \
  --env-file "$ENV_FILE" \
  -f deploy/docker-compose.production.yml \
  up -d --build

for _ in {1..120}; do
  if curl -fsS \
    "http://127.0.0.1:$WEB_PORT/health" \
    >/dev/null 2>&1; then
    break
  fi
  sleep 2
done

if ! curl -fsS \
  "http://127.0.0.1:$WEB_PORT/health" \
  >/dev/null; then
  docker compose \
    --project-name "$PROJECT_NAME" \
    --env-file "$ENV_FILE" \
    -f deploy/docker-compose.production.yml \
    ps

  echo "Production deployment did not become healthy."
  exit 1
fi

echo
echo "FieldOps Hub production deployment is healthy:"
echo "  http://127.0.0.1:$WEB_PORT"
echo
echo "Use STOP_PRODUCTION_DEMO.command to stop it."

if [[ "$(uname -s)" == "Darwin" ]]; then
  open "http://127.0.0.1:$WEB_PORT"
fi
