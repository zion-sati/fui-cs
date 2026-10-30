#!/usr/bin/env bash
# Pins @effindomv2/runtime, the runtime FUI-CS builds against and FuiCs.Runtime repacks, to EFFINDOM_RUNTIME_VERSION
# (npm's latest otherwise), and refreshes the package lock.
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "${repo_root}"
runtime_version="${EFFINDOM_RUNTIME_VERSION:-$(npm view @effindomv2/runtime dist-tags.latest)}"
if [[ ! "${runtime_version}" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
  echo "Invalid EffinDOM runtime version: ${runtime_version}" >&2
  exit 1
fi
node --input-type=module - "${runtime_version}" <<'NODE'
import { readFileSync, writeFileSync } from 'node:fs';
const [version] = process.argv.slice(2);
const packageJson = JSON.parse(readFileSync('package.json', 'utf8'));
packageJson.dependencies['@effindomv2/runtime'] = version;
writeFileSync('package.json', `${JSON.stringify(packageJson, null, 2)}\n`);
NODE
npm install --package-lock-only --ignore-scripts --no-audit --no-fund
echo "Pinned @effindomv2/runtime@${runtime_version}."
