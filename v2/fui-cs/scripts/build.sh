#!/usr/bin/env bash

set -euo pipefail

PACKAGE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
REPO_ROOT="$(cd "${PACKAGE_DIR}/../.." && pwd)"
OUT_DIR="${REPO_ROOT}/public/v2/fui-cs"
BRIDGE_DIR="${REPO_ROOT}/public/v2/browser-bridge"
PUBLISH_DIR="${PACKAGE_DIR}/build/publish"

if ! command -v dotnet >/dev/null 2>&1 && [ -f /etc/paths.d/dotnet ]; then
  export PATH="$(cat /etc/paths.d/dotnet):${PATH}"
fi
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found. Install the .NET SDK and ensure dotnet is on PATH." >&2
  exit 1
fi

rm -rf "${PUBLISH_DIR}"
mkdir -p "${PACKAGE_DIR}/build" "${OUT_DIR}"

dotnet publish "${PACKAGE_DIR}/browser-app/FuiCs.Browser.csproj" \
  -c Release \
  -maxcpucount:1 \
  -nodeReuse:false \
  -o "${PUBLISH_DIR}"

WEB_SOURCE="${PUBLISH_DIR}/wwwroot"
if [ ! -d "${WEB_SOURCE}" ]; then
  WEB_SOURCE="${PUBLISH_DIR}"
fi

rm -rf "${OUT_DIR}"
mkdir -p "${OUT_DIR}"
cp -R "${WEB_SOURCE}/." "${OUT_DIR}/"

cat > "${OUT_DIR}/effindom-runtime-config.js" << 'RUNTIMECFG'
window.__effindomRuntime = Object.assign({}, window.__effindomRuntime, {
  manifestUrl: './effindom.v2.manifest.json',
});
RUNTIMECFG

cp "${BRIDGE_DIR}/bridge.js" "${OUT_DIR}/bridge.js"
cp "${BRIDGE_DIR}/bridge.js.map" "${OUT_DIR}/bridge.js.map"
cp "${BRIDGE_DIR}/effindom.v2.manifest.json" "${OUT_DIR}/effindom.v2.manifest.json"
if [ -f "${BRIDGE_DIR}/icu-asset.json" ]; then
  cp "${BRIDGE_DIR}/icu-asset.json" "${OUT_DIR}/icu-asset.json"
fi
rm -rf "${OUT_DIR}/runtime"
cp -R "${BRIDGE_DIR}/runtime" "${OUT_DIR}/runtime"
