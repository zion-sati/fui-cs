# FUI-CS Contributor Quickstart

This guide is for people working on the SDK itself: its C#, the generated
bindings, the browser integration, the tests and the demo. To build
applications, start with the [Quickstart](./QUICKSTART.md).

## Prerequisites

- The .NET SDK pinned in `global.json` (a .NET 11 SDK; FUI-CS uses C# 15)
- Node.js 24+ and npm, for the TypeScript side of the build and the browser
  tests; the build runs `npm ci` itself and the browser tests install Chromium

No .NET workloads are needed. The first restore downloads the NetWasm SDK
pinned in `global.json` and its host tools (Node, `wasm-ld`, `wasm-opt`)
through NuGet. The npm scripts' `v2/fui-cs/scripts/dotnet-env.sh` also finds
`dotnet` through the macOS installer's `/etc/paths.d/dotnet` or in `~/.dotnet`.

## Build and run

`dotnet` drives everything, from the repository root (or `v2/fui-cs`):

```bash
dotnet build                                      # the SDK, the demo, the tests
dotnet publish v2/fui-cs/demo/Home -c Release     # the demo, to public/v2/fui-cs/demo/
dotnet publish v2/fui-cs/browser-app -c Release   # the test pages, to public/v2/fui-cs/
dotnet test                                       # unit and browser tests
dotnet pack                                       # the packages, to v2/fui-cs/build/packages/
```

The build also does the TypeScript side, incrementally
(`build-support/FuiCs.JavaScript.proj`): `npm ci` when a lock changed, the
generated ABI bindings, the host generator tools and the JSImport modules. The
demo's dashboard route publishes the whole routed demo through `FuiCsRoute`
items, the same way an application from `dotnet new fui-routed-app` does. The
npm scripts are aliases for these: `npm run build` checks the docs and
publishes both sites, and `npm run serve` serves `public/`.

The demo's shell and the test worker define their host services and events
once in TypeScript (`defineHostServices`, `defineHostEvents`), as the other FUI
SDKs' demos do, and their projects' `FuiCsHostServices` and `FuiCsHostEvents`
items generate the C# side as they build. Applications call their page's
JavaScript with `[JSImport]` instead
([Host services, host events and workers](./HOST_SERVICES_AND_WORKERS.md)).

## Tests

```bash
dotnet test                                                  # everything below but the package test
dotnet test v2/fui-cs/tests/FuiCs.Tests -f net11.0            # the desktop unit lane alone
dotnet test v2/fui-cs/tests/FuiCs.BrowserTests                # the browser tests alone
dotnet test v2/fui-cs/tests/FuiCs.PackageTests                # pack, then build an application from each template
```

The unit tests (`v2/fui-cs/tests/FuiCs.Tests`) are TUnit tests that drive the
retained tree, event routing, controls and the application shell against a
call-recording ABI (`FuiCs.Testing.RecordingFuiAbi`). They run in two lanes,
and a change is done only when both pass:

- **desktop**: on .NET, for fast iteration (`-f net11.0`);
- **netwasm**: compiled to WebAssembly with NetWasm and run there, which
  catches compiler-specific behavior (`-f netwasm0.1`).

The NetWasm lane supports a subset of TUnit's assertions. The tests use
`IsEqualTo`, `IsTrue`, `IsFalse`, `IsNull` and `IsNotNull`, join collections
into strings to compare them, and check exceptions with a small `Throws<T>`
helper.

The browser tests (`v2/fui-cs/tests/*.spec.ts`) run in Chromium against the
published test pages and demo; `FuiCs.BrowserTests` publishes both, then runs
each spec file as one test. Run one file against the published sites with:

```bash
npx playwright test -c v2/fui-cs/playwright.config.ts v2/fui-cs/tests/Dropdown.spec.ts
```

`npm run typecheck` checks the TypeScript under `v2/fui-cs`.

## Generated code

Generated files are committed. The build regenerates the code ones whenever
their sources change, so a `dotnet build` leaves them current:

| Generated | Writes | From |
|---|---|---|
| ABI bindings (every build, `build-support/FuiCs.JavaScript.proj`) | the ABI interface and recording ABI (`src/FuiCs/Generated/`), the JSImport declarations and module list (`src/FuiCs.Exports/`), and the browser import modules (`browser/imports/generated/`) | the C headers `v2/ui/include/effindom_ui.h` and `v2/core/include/effindom.h`, through `v2/abi/generate-fui-cs.ts` |
| Host bindings (each project's build) | the project's `Host/*.g.cs` and services module | the TypeScript definitions its `FuiCsHostServices` and `FuiCsHostEvents` items name |
| `npm run generate:docs` | `docs/v2/fui-cs/reference/` | the XML documentation of `FuiCs` and `FuiCs.Worker` |

`npm run check:generated` builds with `FuiCsCheckGenerated=true`, which fails on
an out-of-date generated file instead of rewriting it, as CI does;
`check:docs` does the same for the reference, and `check:examples` compiles the
guides' C# examples.

The ABI generator emits only the runtime calls the hand-written C# uses: to
use a new runtime function, call it from the SDK's C# (through `Runtime.Abi`)
and build.

A guide's ```` ```csharp ```` blocks compile as part of `check:examples`. An HTML
comment on the line before a block says what it needs:
`<!-- example: TextInput nameField, Action Save -->` names the variables it
assumes, `<!-- example: worker -->` compiles it as worker-project code, and
`<!-- example: skip -->` leaves out a fragment that cannot compile alone, such
as one using generated bindings.

## Repository layout

```text
v2/fui-cs/
  src/FuiCs/           the SDK
    Core/              nodes, events, routing, focus, application, theme
    Nodes/             FlexBox, Grid, ScrollView, ScrollBox, Image, Svg, ...
    Text/              Text, RichText, TextLayout, fonts
    Controls/          Button, toggles, Slider, Dropdown, ComboBox, TextInput, Dialog, ...
    Drawing/           DrawContext, Paint, PathGeometry, Bitmap
    Assets/            texture and SVG assets
    Platform/          platform services, navigation, fetch, persistence, workers
    Bindings/, Abi/    the internal runtime wrappers, the ABI interface and the recording ABI
    Generated/         generated ABI projection
    Hosting/           the host's entry points
  src/FuiCs.Exports/   the exports and entry point compiled into each application
  src/FuiCs.Worker/    the worker-project library (FuiCs.Workers)
  browser/             the TypeScript harness, adapter and import modules
  browser-app/         the test app's pages, which the browser tests drive
  browser-worker/      the test app's worker
  demo/                the demo: one application per route, and its design system
  tests/               FuiCs.Tests (unit), the Playwright specs, FuiCs.BrowserTests (runs them under dotnet
                       test) and FuiCs.PackageTests (the packages and templates)
  packaging/           the package projects, the web targets and the templates
  build-support/       the build's TypeScript side, the desktop test lane and package staging
  scripts/             npm aliases and generator scripts
docs/v2/fui-cs/        the guides and the generated reference
v2/abi/                the ABI generator (shared model and the fui-cs projection)
```

## Conventions

- **API shape**: state is a property with a fluent member of the same name;
  input and changes are C# events with an `On...` fluent member; values a user
  produces are clamped and programming errors throw. See
  [API conventions](./API_REFERENCE.md).
- **Docs**: the SDK builds with XML documentation and the recommended .NET
  analyzers, warning-free. Every public member has a doc comment; regenerate
  the reference after changing one. A change to the public surface updates
  the guide that describes it.
- **Tests**: new behavior comes with unit tests in both lanes, and a browser
  test when it crosses into the browser (input, text editing, focus, layout
  read back from the runtime).
- **TypeScript**: strict, with the type-aware ESLint rules; no `any`.
- **Size**: applications build with `NetWasmOptimization=Oz`, and every SDK
  line that an application reaches ships in its wasm. The SDK uses no LINQ or
  reflection; check the published size of a demo route after a change that
  adds a dependency.
