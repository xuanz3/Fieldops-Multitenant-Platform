#!/usr/bin/env sh
set -eu

echo "Applying FieldOps migrations and fictional demonstration data..."
dotnet FieldOps.Api.dll --seed-demo

echo "Starting FieldOps API..."
exec dotnet FieldOps.Api.dll
