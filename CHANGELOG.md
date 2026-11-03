# Changelog

## 0.4.1 - 2026-11-03

- FUI-CS pages are titled by their route (FuiCsHomeTitle, or a FuiCsRoute's Title) instead of "FuiCs application", and a routed site's title follows the route as links swap it, until the application sets a caption; the canvas fills the visible viewport on phones, where it was as tall as the screen behind the browser's address bar; and dotnet run serves a routed site, which stopped with NWSDK003 on its routes.
- A Release build of a FUI-CS site runs the EffinDOM runtime in release mode, as FUI-AS and FUI-RS publish, so its developer tools stay off; the new FuiCsDevToolsDomMirror property (disabled, enabled or on-requested) sets them, like fui-config.json's web.devTools.domMirror. Debug builds and dotnet run keep the tools on request.
- Scroll views latch mouse-wheel runs as browsers do: a wheel run that starts in a scroll view stays in it at its edges, rather than scrolling an ancestor mid-run, until the wheel pauses for 150 ms, and the page does not scroll meanwhile; a trackpad gesture keeps its scroll view until it ends. It is on by default; turn it off per scroll view or scroll box with wheelLatching(false) in FUI-AS, wheel_latching(false) in FUI-RS or WheelLatching = false in FUI-CS (the runtime's ui_set_scroll_wheel_latching).

## 0.4.0 - 2026-11-03

- FUI-CS pages show the loading overlay as FUI-AS and FUI-RS pages do: from the first moments of a slow load, with the runtime asset and built-in font progress, then the application's WebAssembly by name, instead of a placeholder module's data: URL.
- FuiCs is built with NetWasm 0.10.7 and the .NET 11 RC3 SDK, and new projects from the fui-app and fui-routed-app templates use them. Applications on NetWasm 0.10.6 keep working; moving one to 0.10.7 (the NetWasm.Sdk version in its project files, and the RC3 SDK in global.json) makes its WebAssembly smaller, by about 6 to 9% after Brotli across the FuiCs demo's routes.
- Sliders and progress bars can fill their container along their orientation (fill_length in FUI-RS, fillLength() in FUI-AS, FillLength()/FillsLength in FUI-CS) instead of a fixed length; a filling slider maps the pointer over the length layout gives it. Slider presenters lay out relative to their root, so the default slider renders as before at a fixed length.

## 0.3.0 - 2026-10-30

- First release of FUI-CS, the C# SDK for EffinDOM on NetWasm: the FuiCs, FuiCs.Worker and FuiCs.Runtime NuGet packages, and FuiCs.Templates with the dotnet new fui-app and fui-routed-app templates. An application builds and publishes as a static site with the .NET SDK alone.
