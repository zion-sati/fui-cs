# Changelog

## 0.4.0 - 2026-11-03

- FUI-CS pages show the loading overlay as FUI-AS and FUI-RS pages do: from the first moments of a slow load, with the runtime asset and built-in font progress, then the application's WebAssembly by name, instead of a placeholder module's data: URL.
- FuiCs is built with NetWasm 0.10.7 and the .NET 11 RC3 SDK, and new projects from the fui-app and fui-routed-app templates use them. Applications on NetWasm 0.10.6 keep working; moving one to 0.10.7 (the NetWasm.Sdk version in its project files, and the RC3 SDK in global.json) makes its WebAssembly smaller, by about 6 to 9% after Brotli across the FuiCs demo's routes.
- Sliders and progress bars can fill their container along their orientation (fill_length in FUI-RS, fillLength() in FUI-AS, FillLength()/FillsLength in FUI-CS) instead of a fixed length; a filling slider maps the pointer over the length layout gives it. Slider presenters lay out relative to their root, so the default slider renders as before at a fixed length.

## 0.3.0 - 2026-10-30

- First release of FUI-CS, the C# SDK for EffinDOM on NetWasm: the FuiCs, FuiCs.Worker and FuiCs.Runtime NuGet packages, and FuiCs.Templates with the dotnet new fui-app and fui-routed-app templates. An application builds and publishes as a static site with the .NET SDK alone.
