# FUI-CS - C# SDK for EffinDOM

> **Early stage.** FUI-CS is under active development and its API will change.

**[→ Live demo](https://fui-cs-demo.effindom.dev/)**, built from these packages
with `dotnet publish` ([source](https://github.com/zion-sati/fui-cs-demo/tree/main/fui-cs-demo-source)).

FUI-CS builds EffinDOM applications in C#. An application is a tree of retained
nodes and controls (layout boxes, text, images, buttons, toggles, sliders,
dropdowns, combo boxes, text inputs, dialogs, menus, popups, tooltips, virtual
lists, custom drawing and bitmaps) with C# events, a theme and fluent
builders. It compiles to WebAssembly with [NetWasm](https://github.com/zion-sati/NetWasm)
as a `JsModule` and drives the EffinDOM UI runtime in the browser.

```csharp
using FuiCs;

namespace MyApp;

static class FuiProgram
{
    public static IApplication CreateApplication() => Application.Create(BuildPage);

    static Node BuildPage()
    {
        var greeting = new Text("Hello").FontSize(20.0f);
        return FlexBox.Column(greeting, new Button("Greet").OnClick(_ => greeting.Text = "Hello, C#"))
            .Padding(24.0f);
    }
}
```

## Applications

An application needs only the .NET SDK:

```bash
dotnet new install FuiCs.Templates
dotnet new fui-app -o MyApp        # or fui-routed-app, a shell with two routes
cd MyApp
dotnet publish -c Release -o site
```

`site/` is a static site; serve it at the root of any static file server.

## Working on FUI-CS

Prerequisites: the .NET SDK pinned in `global.json`, and Node.js 24+ with npm,
which the build uses for its TypeScript side. No .NET workloads are needed:
NetWasm restores its own Node, `wasm-ld` and `wasm-opt` through NuGet.

```bash
git clone https://github.com/zion-sati/fui-cs.git
cd fui-cs
dotnet build
dotnet test
dotnet publish v2/fui-cs/demo/Home -c Release
npm run serve
```

Open `http://127.0.0.1:8080/v2/fui-cs/demo/` for the demo.

The repository does not contain or build the EffinDOM runtime; it uses the
published `@effindomv2/runtime` package.

## Docs

- [Quickstart](docs/v2/fui-cs/QUICKSTART.md)
- [SDK docs index](docs/v2/fui-cs/SDK_INDEX.md)
- [API reference](docs/v2/fui-cs/reference/README.md)
- [Contributor quickstart](docs/v2/fui-cs/CONTRIBUTOR_QUICKSTART.md)

## License

AGPL-3.0-only (or commercial - see [COMMERCIAL.md](v2/fui-cs/COMMERCIAL.md)).
