#!/bin/zsh
set -e
ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
chmod +x "$ROOT_DIR/scripts/start-production-demo.sh"
exec "$ROOT_DIR/scripts/start-production-demo.sh"
