#!/usr/bin/env bash
# usage: test-unit.sh [all|desktop|netwasm]   (default: all)
# desktop runs the TUnit project on .NET for fast iteration; netwasm compiles the same tests with
# NetWasm and runs them as WebAssembly. A phase is done only when both lanes pass.

set -euo pipefail

PACKAGE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="${PACKAGE_DIR}/tests/FuiCs.Tests/FuiCs.Tests.csproj"
LANE="${1:-all}"

source "${PACKAGE_DIR}/scripts/dotnet-env.sh"

case "${LANE}" in
  all|desktop|netwasm) ;;
  *)
    echo "Unknown test lane: ${LANE} (expected all, desktop or netwasm)" >&2
    exit 1
    ;;
esac

# dotnet test runs both lanes; build-support/DesktopTUnit.targets runs the desktop lane's TUnit executable.
case "${LANE}" in
  all) dotnet test "${PROJECT}" ;;
  desktop) dotnet test "${PROJECT}" -f net11.0 ;;
  netwasm) dotnet test "${PROJECT}" -f netwasm0.1 ;;
esac
