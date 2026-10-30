# Puts dotnet on PATH for fui-cs scripts. Source it; it exits the caller when dotnet is missing.
# Non-login shells may miss the macOS installer's /etc/paths.d/dotnet or a user-local ~/.dotnet.

if ! command -v dotnet >/dev/null 2>&1 && [ -f /etc/paths.d/dotnet ]; then
  export PATH="$(cat /etc/paths.d/dotnet):${PATH}"
fi
if ! command -v dotnet >/dev/null 2>&1 && [ -x "${HOME}/.dotnet/dotnet" ]; then
  export DOTNET_ROOT="${HOME}/.dotnet"
  export PATH="${DOTNET_ROOT}:${PATH}"
fi
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found. Install the .NET SDK pinned in v2/fui-cs/global.json and ensure dotnet is on PATH." >&2
  exit 1
fi
export DOTNET_CLI_TELEMETRY_OPTOUT="${DOTNET_CLI_TELEMETRY_OPTOUT:-1}"
export DOTNET_NOLOGO="${DOTNET_NOLOGO:-1}"
