# Contributing to FUI-CS

> **Early stage.** FUI-CS is a smoke-level C# SDK over the shared EffinDom v2
> C ABI. It is under active development and will change.

This guide is for people working on the SDK itself.

## Prerequisites

- .NET 10 SDK
- `wasm-tools` and `wasm-experimental` workloads
- Node.js 24+ and npm

```bash
dotnet workload install wasm-tools wasm-experimental
```

## Clone and Build

```bash
git clone https://github.com/zion-sati/EffinDOM.git
cd EffinDOM
npm ci
npm run build:v2:browser-bridge
npm run build:v2:fui-cs
```

## Run Tests

```bash
npm run test:v2:fui-cs:unit
npm run test:v2:fui-cs:integration
```

The integration test loads the C# smoke app through Playwright with the shared
v2 browser bridge runtime.

## Run the Smoke App

```bash
npm run serve
```

Open `http://127.0.0.1:8080/v2/fui-cs/index.html`.

## Repo Structure

```text
src/FuiCs/
  Ffi.cs          - backend abstraction and shared enums
  Node.cs         - fluent node builders
  Application.cs  - mount entrypoint
  Smoke.cs        - smoke tree used by tests/browser app

browser-app/
  Program.cs      - wasmbrowser JS interop backend
  wwwroot/        - smoke harness page and JS bootstrap

tests/
  FuiCs.Tests/    - managed call-recording tests
  Smoke.spec.ts   - browser smoke test
```

## Docs

- [FUI-CS Quickstart](https://github.com/zion-sati/EffinDOM/blob/main/docs/v2/fui-cs/QUICKSTART.md)
- [FUI-AS Docs Index](https://github.com/zion-sati/EffinDOM/blob/main/docs/v2/fui-as/SDK_INDEX.md) (C ABI docs apply to all SDKs)

## Getting in Touch

This is a solo project. If you're thinking about contributing, please open an
issue or start a discussion before writing code.

For anything else: **zionsatidev@gmail.com**
