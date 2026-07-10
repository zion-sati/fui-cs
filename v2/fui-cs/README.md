# FUI-CS

Smoke-level C# bindings for EffinDom v2.

This package intentionally starts at parity with `fui-rs` and `fui-kt`: a tiny
builder surface, native call-recording tests, and a browser smoke app.

The browser target uses the lightweight .NET `wasmbrowser` path rather than
Blazor. It needs the .NET WebAssembly workloads:

```sh
sudo dotnet workload install wasm-tools wasm-experimental
```

In this repository, non-login shells may not pick up `/etc/paths.d/dotnet`; if
`dotnet` is missing, open a new terminal or ensure your shell PATH includes the
dotnet install path.
