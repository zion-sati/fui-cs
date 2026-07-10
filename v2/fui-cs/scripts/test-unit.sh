#!/usr/bin/env bash

set -euo pipefail

PACKAGE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if ! command -v dotnet >/dev/null 2>&1 && [ -f /etc/paths.d/dotnet ]; then
  export PATH="$(cat /etc/paths.d/dotnet):${PATH}"
fi
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found. Install the .NET SDK and ensure dotnet is on PATH." >&2
  exit 1
fi

dotnet test "${PACKAGE_DIR}/tests/FuiCs.Tests/FuiCs.Tests.csproj"
