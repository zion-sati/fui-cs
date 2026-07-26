# FUI-CS - C# bindings for EffinDom v2

> **Early stage.** FUI-CS is a thin C# binding over the shared EffinDom v2 UI ABI.
> It currently provides a smoke-level fluent builder, call-recording tests, and
> a browser smoke app using the lightweight .NET `wasmbrowser` path.
> Expect breaking changes.

## Quickstart

```bash
# Prerequisites: .NET 10 SDK + WebAssembly workloads
dotnet workload install wasm-tools wasm-experimental

# Clone and build against the published EffinDOM runtime
git clone https://github.com/zion-sati/fui-cs.git
cd fui-cs
npm install
npm run build
npm run serve
```

Open `http://127.0.0.1:8080/v2/fui-cs/index.html`.

Full quickstart: [docs/v2/fui-cs/QUICKSTART.md](docs/v2/fui-cs/QUICKSTART.md)

Run the call-recording unit smoke with `npm test`. The repository does not
contain or build the EffinDOM runtime source; `npm run build` stages the
published runtime package before building FUI-CS.

## What's Here

| Primitive | Status |
|---|---|
| `FlexBox` / `TextNode` builders | Implemented |
| C# call-recording unit smoke | Implemented |
| Browser smoke through EffinDom bridge | Implemented |
| Controls, theming, signals, reconciliation | Planned |

## Runtime Shape

The browser target uses .NET's lightweight `wasmbrowser` template, not Blazor.
The default publish mode is non-AOT + trimmed. This keeps the shared .NET
runtime assets reusable across pages while keeping the app-specific C# payload
small.

## License

AGPL-3.0-only (or commercial - see [COMMERCIAL.md](COMMERCIAL.md)).
