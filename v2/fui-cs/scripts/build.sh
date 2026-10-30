#!/usr/bin/env bash
# Checks the docs, then publishes the test pages to public/v2/fui-cs/ and the demo to public/v2/fui-cs/demo/. The
# projects' own builds do the rest (build-support/FuiCs.JavaScript.proj, host generation, the web shell), so this is
# the same as running the two dotnet publish commands below.

set -euo pipefail

PACKAGE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
REPO_ROOT="$(cd "${PACKAGE_DIR}/../.." && pwd)"
OUT_DIR="${REPO_ROOT}/public/v2/fui-cs"

source "${PACKAGE_DIR}/scripts/dotnet-env.sh"

# The API reference must match the XML docs, and the guides' C# examples must compile.
(cd "${PACKAGE_DIR}" && npm run --silent check:docs)
(cd "${PACKAGE_DIR}" && npm run --silent check:examples)

# Publishing adds to its folder, so start from an empty one.
rm -rf "${OUT_DIR}"

# The test pages, with the test worker in browser/workers/FuiCs.TestWorker/.
dotnet publish "${PACKAGE_DIR}/browser-app/FuiCs.Browser.csproj" -c Release
# The demo: the dashboard route publishes every other route beside it.
dotnet publish "${PACKAGE_DIR}/demo/Home/FuiCs.Demo.Home.csproj" -c Release
