#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(
  cd "$(dirname "${BASH_SOURCE[0]}")/.."
  pwd
)"
FRONTEND_DIR="$ROOT_DIR/src/frontend/fieldops-web"
RUNTIME_DIR="$ROOT_DIR/.fieldops-runtime"
API_LOG="$RUNTIME_DIR/api.log"
WEB_LOG="$RUNTIME_DIR/web.log"
ENV_FILE="$RUNTIME_DIR/local.env"
COMPOSE_PROJECT="fieldops-hub-local"

mkdir -p "$RUNTIME_DIR"

for tool in docker dotnet npm curl python3; do
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

if ! docker info >/dev/null 2>&1; then
  echo "Docker is not ready."
  exit 1
fi

choose_port() {
  local preferred="$1"

  python3 - "$preferred" <<'PY'
import socket
import sys

preferred = int(sys.argv[1])

for port in range(preferred, preferred + 200):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        try:
            sock.bind(("127.0.0.1", port))
        except OSError:
            continue
        print(port)
        raise SystemExit(0)

raise SystemExit("No free local TCP port was found.")
PY
}

POSTGRES_PORT="$(choose_port 5432)"
API_PORT="$(choose_port 5204)"
WEB_PORT="$(choose_port 5173)"

cat >"$ENV_FILE" <<EOF
POSTGRES_DB=fieldops
POSTGRES_USER=fieldops
POSTGRES_PASSWORD=fieldops_dev_password
POSTGRES_PORT=$POSTGRES_PORT
API_PORT=$API_PORT
WEB_PORT=$WEB_PORT
EOF

cleanup() {
  if [[ -n "${API_PID:-}" ]]; then
    kill "$API_PID" 2>/dev/null || true
  fi

  if [[ -n "${WEB_PID:-}" ]]; then
    kill "$WEB_PID" 2>/dev/null || true
  fi
}

trap cleanup EXIT INT TERM

cd "$ROOT_DIR"

echo "Starting PostgreSQL on host port $POSTGRES_PORT..."
POSTGRES_PORT="$POSTGRES_PORT" \
POSTGRES_DB="fieldops" \
POSTGRES_USER="fieldops" \
POSTGRES_PASSWORD="fieldops_dev_password" \
docker compose \
  --project-name "$COMPOSE_PROJECT" \
  up -d postgres

export ConnectionStrings__FieldOps="Host=127.0.0.1;Port=$POSTGRES_PORT;Database=fieldops;Username=fieldops;Password=fieldops_dev_password"
export Authentication__Jwt__Issuer="FieldOps.LocalDemo"
export Authentication__Jwt__Audience="FieldOps.LocalDemo.Client"
export Authentication__Jwt__SigningKey="fieldops-local-demo-signing-key-2026-at-least-forty-eight-characters"
export ASPNETCORE_URLS="http://127.0.0.1:$API_PORT"
export VITE_API_PROXY_TARGET="http://127.0.0.1:$API_PORT"

echo "Restoring and building the backend..."
dotnet restore FieldOps.sln
dotnet build FieldOps.sln \
  --configuration Release \
  --no-restore

echo "Applying migrations and fictional demo data..."
dotnet run \
  --project src/backend/FieldOps.Api/FieldOps.Api.csproj \
  --configuration Release \
  --no-build \
  --no-launch-profile \
  -- \
  --seed-demo

echo "Starting the API..."
dotnet run \
  --project src/backend/FieldOps.Api/FieldOps.Api.csproj \
  --configuration Release \
  --no-build \
  --no-launch-profile \
  >"$API_LOG" 2>&1 &
API_PID=$!

echo "Preparing the frontend..."
cd "$FRONTEND_DIR"

if [[ ! -d node_modules ]]; then
  npm ci
fi

echo "Starting the frontend..."
npm run dev -- \
  --host 127.0.0.1 \
  --port "$WEB_PORT" \
  >"$WEB_LOG" 2>&1 &
WEB_PID=$!

for _ in {1..90}; do
  if curl -fsS \
    "http://127.0.0.1:$API_PORT/health" \
    >/dev/null 2>&1; then
    break
  fi
  sleep 1
done

for _ in {1..90}; do
  if curl -fsS \
    "http://127.0.0.1:$WEB_PORT/" \
    >/dev/null 2>&1; then
    break
  fi
  sleep 1
done

if ! curl -fsS \
  "http://127.0.0.1:$API_PORT/health" \
  >/dev/null; then
  echo "API failed to start. See: $API_LOG"
  exit 1
fi

if ! curl -fsS \
  "http://127.0.0.1:$WEB_PORT/" \
  >/dev/null; then
  echo "Frontend failed to start. See: $WEB_LOG"
  exit 1
fi

echo
echo "FieldOps Hub is running:"
echo "  Frontend: http://127.0.0.1:$WEB_PORT"
echo "  API:      http://127.0.0.1:$API_PORT"
echo "  Database: 127.0.0.1:$POSTGRES_PORT"
echo
echo "Demo login:"
echo "  Tenant:   northside-property-services"
echo "  Email:    dispatcher@northside.example.test"
echo "  Password: FieldOps-Demo-2026!"
echo
echo "Press Ctrl+C to stop the frontend and API."
echo "PostgreSQL remains available for the next run."
echo "Use STOP_LOCAL_DEMO.command to stop it."

if [[ "$(uname -s)" == "Darwin" ]]; then
  open "http://127.0.0.1:$WEB_PORT"
fi

wait
