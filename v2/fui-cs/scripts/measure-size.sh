#!/usr/bin/env bash
# Publishes the smoke page alone at Oz and prints its Wasm size, the series the port plan tracks.
set -euo pipefail

PACKAGE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
# shellcheck source=dotnet-env.sh
source "${PACKAGE_DIR}/scripts/dotnet-env.sh"

OUT_DIR="$(mktemp -d)"
trap 'rm -rf -- "${OUT_DIR:?}"' EXIT

dotnet publish "${PACKAGE_DIR}/browser-app/FuiCs.Browser.csproj" -c Release -p:FuiCsSmokeOnly=true \
  -p:IntermediateOutputPath="${OUT_DIR}/obj/" -o "${OUT_DIR}/publish" -nologo -v q > "${OUT_DIR}/publish.log" 2>&1 \
  || { cat "${OUT_DIR}/publish.log"; exit 1; }

WASM="${OUT_DIR}/publish/browser/FuiCs.Browser.wasm"
node -e '
const fs = require("fs"), zlib = require("zlib");
const bytes = fs.readFileSync(process.argv[1]);
const brotli = zlib.brotliCompressSync(bytes, { params: { [zlib.constants.BROTLI_PARAM_QUALITY]: 11 } }).length;
console.log(`smoke wasm (Oz): ${bytes.length} raw, ${brotli} brotli`);
' "${WASM}"
