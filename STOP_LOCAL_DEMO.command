#!/bin/zsh
set -e
ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
chmod +x "$ROOT_DIR/scripts/stop-local-demo.sh"
exec "$ROOT_DIR/scripts/stop-local-demo.sh"
