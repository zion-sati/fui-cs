#!/usr/bin/env bash
# Generates the per-type API reference in docs/v2/fui-cs/reference from the XML documentation of the SDK and its
# worker library.
# --check fails when the reference is out of date instead of writing it.

set -euo pipefail

PACKAGE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
# shellcheck source=dotnet-env.sh
source "${PACKAGE_DIR}/scripts/dotnet-env.sh"
TOOLS_DIR="${PACKAGE_DIR}/build/docgen"
CHECK=()
if [ "${1:-}" = "--check" ]; then
  CHECK=(--check)
fi

dotnet build "${PACKAGE_DIR}/src/FuiCs/FuiCs.csproj" -f net11.0 -nologo -v q > /dev/null
dotnet build "${PACKAGE_DIR}/src/FuiCs.Worker/FuiCs.Worker.csproj" -f net11.0 -nologo -v q > /dev/null
mkdir -p "${TOOLS_DIR}"
npx esbuild "${PACKAGE_DIR}/scripts/generate-api-reference.ts" --bundle --format=esm --platform=node --target=node20 \
  --packages=external --log-level=warning --outfile="${TOOLS_DIR}/generate-api-reference.mjs"
node "${TOOLS_DIR}/generate-api-reference.mjs" "${PACKAGE_DIR}/../../docs/v2/fui-cs/reference" \
  "${PACKAGE_DIR}/src/FuiCs/bin/Debug/net11.0/FuiCs.xml" "${PACKAGE_DIR}/src/FuiCs" \
  "${PACKAGE_DIR}/src/FuiCs.Worker/bin/Debug/net11.0/FuiCs.Worker.xml" "${PACKAGE_DIR}/src/FuiCs.Worker" "${CHECK[@]}"
