#!/usr/bin/env bash
# Runs dotnet with the SDK global.json pins, for npm scripts in shells whose PATH lacks a user-local dotnet.
set -euo pipefail
source "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/dotnet-env.sh"
exec dotnet "$@"
