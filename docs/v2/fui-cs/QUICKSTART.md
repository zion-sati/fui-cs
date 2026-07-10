# FUI-CS Quickstart

FUI-CS is the smoke-level C# SDK for EffinDom v2. It uses .NET's lightweight
`wasmbrowser` path and talks to the same retained-mode C++ UI/runtime bridge as
FUI-AS, FUI-RS, and FUI-KT.

## Prerequisites

- .NET 10 SDK
- .NET WebAssembly workloads
- Node.js 24+ and npm

```bash
dotnet workload install wasm-tools wasm-experimental
```

## Build

From the repository root:

```bash
npm ci
npm run build:v2:browser-bridge
npm run build:v2:fui-cs
```

The C# build stages the smoke app at:

```text
public/v2/fui-cs/index.html
```

## Run

```bash
npm run serve
```

Open:

```text
http://127.0.0.1:8080/v2/fui-cs/index.html
```

## Tests

```bash
npm run test:v2:fui-cs:unit
npm run test:v2:fui-cs:integration
```

The unit test checks the C# fluent builder against a call-recording backend.
The integration test boots the published `wasmbrowser` app through the shared
EffinDom browser bridge.

## Publish Shape

The default browser publish mode is:

```xml
<PublishTrimmed>true</PublishTrimmed>
<RunAOTCompilation>false</RunAOTCompilation>
```

This is intentionally non-AOT so the shared .NET runtime artifacts remain more
cacheable across pages. The C# app-specific payload is small, while shared
fingerprinted framework files can be served with long-lived immutable caching.
