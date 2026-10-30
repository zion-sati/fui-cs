#!/usr/bin/env bash
# Compiles the C# examples of the public guides in docs/v2/fui-cs against the SDK; fails when one does not compile.

set -euo pipefail

PACKAGE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
# shellcheck source=dotnet-env.sh
source "${PACKAGE_DIR}/scripts/dotnet-env.sh"
TOOLS_DIR="${PACKAGE_DIR}/build/docgen"
PROJECT_DIR="${PACKAGE_DIR}/build/doc-examples"

mkdir -p "${TOOLS_DIR}"
npx esbuild "${PACKAGE_DIR}/scripts/check-doc-examples.ts" --bundle --format=esm --platform=node --target=node20 \
  --packages=external --log-level=warning --outfile="${TOOLS_DIR}/check-doc-examples.mjs"
node "${TOOLS_DIR}/check-doc-examples.mjs" "${PACKAGE_DIR}/../../docs/v2/fui-cs" "${PROJECT_DIR}"
dotnet build "${PROJECT_DIR}/DocExamples.csproj" -nologo -v q -clp:NoSummary
