#!/usr/bin/env bash
# The package test: packs the fui-cs packages and checks an application made from each template against them
# (tests/FuiCs.PackageTests). The same as running dotnet test on that project.

set -euo pipefail

PACKAGE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
source "${PACKAGE_DIR}/scripts/dotnet-env.sh"

dotnet test "${PACKAGE_DIR}/tests/FuiCs.PackageTests/FuiCs.PackageTests.csproj" -nologo
