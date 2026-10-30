# FuiCs

The C# SDK for EffinDOM: retained nodes and controls (layout, text, images,
buttons, toggles, sliders, dropdowns, combo boxes, text inputs, dialogs, menus,
popups, tooltips, virtual lists, custom drawing and bitmaps) with C# events, a
theme and fluent builders. Applications compile to WebAssembly with NetWasm as
a `JsModule` and run on the EffinDOM UI runtime in the browser.

Create an application from the templates:

```bash
dotnet new install FuiCs.Templates
dotnet new fui-app -o MyApp
cd MyApp
dotnet publish -c Release -o site
```

`site/` is a static site: serve it with any static file server. Publishing
stages the EffinDOM runtime from the `FuiCs.Runtime` package, the application
harness and `index.html` (unless the application publishes its own).

Docs: <https://github.com/zion-sati/fui-cs/blob/main/docs/v2/fui-cs/SDK_INDEX.md>

Live demo, built from these packages: <https://fui-cs-demo.effindom.dev/>

License: AGPL-3.0-only, or the commercial terms in the repository's
`v2/fui-cs/COMMERCIAL.md`.
