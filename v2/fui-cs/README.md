# FUI-CS

The C# SDK for EffinDom v2, with native call-recording tests and a browser
smoke app.

The browser target is a [NetWasm](https://github.com/zion-sati/NetWasm)
`JsModule`. `global.json` pins the .NET SDK and the `NetWasm.Sdk` version; no
.NET workloads are needed, and NetWasm restores its own Node, `wasm-ld` and
`wasm-opt` through NuGet.

`scripts/dotnet-env.sh` finds `dotnet` on `PATH`, through the macOS installer's
`/etc/paths.d/dotnet`, or in a user-local `~/.dotnet`.

The guides are in [`docs/v2/fui-cs`](../../docs/v2/fui-cs/SDK_INDEX.md), from
the [Quickstart](../../docs/v2/fui-cs/QUICKSTART.md) to the generated
[API reference](../../docs/v2/fui-cs/reference/README.md).
