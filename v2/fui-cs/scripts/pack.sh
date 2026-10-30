#!/usr/bin/env bash
# Packs the fui-cs NuGet packages into build/packages: dotnet pack of the solution, which stages what each package
# carries (build-support/FuiCs.Packaging.targets). Nothing is pushed anywhere.
#
# usage: pack.sh [--runtime <dist directory>] [--version <version>]
#
# --version sets the version of FuiCs, FuiCs.Worker and FuiCs.Templates (packaging/Package.props's FuiCsVersion with a
# -local suffix otherwise). FuiCs.Runtime takes the version of the runtime it packs: --runtime, an @effindomv2/runtime
# dist folder, or else the repository's own bridge build or the installed @effindomv2/runtime package
# (Directory.Build.props).

set -euo pipefail

PACKAGE_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

source "${PACKAGE_DIR}/scripts/dotnet-env.sh"

PROPERTIES=()
while [ $# -gt 0 ]; do
  case "$1" in
    --runtime) PROPERTIES+=("-p:FuiCsRuntimeDirectory=$(cd "$2" && pwd)/"); shift 2 ;;
    --version) PROPERTIES+=("-p:Version=$2"); shift 2 ;;
    *) echo "Unknown argument: $1 (expected --runtime <dist> or --version <version>)" >&2; exit 1 ;;
  esac
done

rm -rf "${PACKAGE_DIR}/build/packages"
dotnet pack "${PACKAGE_DIR}/FuiCs.slnx" -c Release -nologo "${PROPERTIES[@]}"
echo "Packed into build/packages:"
ls -1 "${PACKAGE_DIR}/build/packages"
