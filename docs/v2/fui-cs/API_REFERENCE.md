# FUI-CS API Conventions

The [per-type reference](./reference/README.md) lists every public type and
member, generated from the SDK's XML documentation (`npm run generate:docs`
in `v2/fui-cs`; `npm run check:docs` fails when it is stale). This page explains
the conventions the API follows, so the reference reads quickly.

## Namespaces

Everything an application uses is in `FuiCs`. A worker project uses
`FuiCs.Workers`, from the `FuiCs.Worker` library: `WorkerEntries`,
`WorkerRuntime`, `WorkerJob` and a worker-side `FetchRequest`. Two namespaces
serve the build and tests rather than applications: `FuiCs.Hosting` holds the
host's entry points, which the generated exports call, and `FuiCs.Testing`
holds `RecordingFuiAbi`, which records runtime calls for unit tests.

A reference page is named after its type's namespace below `FuiCs`:
`Button.md`, `Workers.FetchRequest.md`.

## Properties and fluent members

State is a settable property with a WPF-style name (`IsChecked`, `Background`,
`Padding`, `SelectedIndex`). Every property also has a fluent member of the
same name that sets it and returns the node as its own type, so builders read
as one expression:

```csharp
var save = new Button("Save").Width(120.0f).Padding(12.0f, 8.0f).ToolTip("Save the file");
save.IsEnabled = false; // the same state, set later
```

The fluent members live in `*Extensions` classes (`ButtonExtensions`,
`FlexBoxExtensions`, ...) as C# 14 extension members, generic over the node
type so a subclass keeps its type through the chain.

Nullable properties mean "follow the default": `null` returns a color to the
theme's, a size to the template's, or a limit to none.

## Events

Input and changes are multicast C# events (`Click`, `Changed`, `TextChanged`,
`PointerDown`, ...) whose arguments derive from `EventArgs`. Each event has an
`On...` fluent member that subscribes and returns the node. The toggles,
radio groups and sliders report every change, from the user or from code; the
dropdown, combo box and text inputs report only the user's, so code that sets
their value does not re-enter its own handlers. Each event's documentation says
which. See [Events and callbacks](./EVENTS_AND_CALLBACKS.md).

## Misuse exceptions

Values that are programming errors throw instead of being clamped: an index
outside the items (`ArgumentOutOfRangeException`), a non-positive size, a
negative limit, a second `SendAsync` on a request (`InvalidOperationException`),
or measuring a `TextLayout` before it is ready. Values a user produces, such as
a restored selection that no longer fits the items, are clamped.

## Disposal

Nodes, layouts, bitmaps, assets, workers and animations are `IDisposable`.
Disposing a node disposes its subtree and deletes its native nodes; mounting a
new root disposes the previous tree. Subscriptions a node makes to app-wide
signals (`Theme.Active`, viewport size) are released with it.

## Signals

`Signal<T>` holds app-wide state (`Theme.Active`, `Application.ViewportWidth`,
`FocusVisibility.KeyboardFocusVisible`, a `ScrollState`'s offsets). `Value`
reads and sets it, `Changed` reports changes, and `Subscribe` returns an
`IDisposable`, which a node subclass releases with it through `Track`.

## Positions and units

Lengths are pixels unless a `Length` says otherwise (`Length.Percent(50)`,
`Length.Auto`); a `float` converts to pixels. Colors are `Color` values; a
`uint` in `0xRRGGBBAA` form converts to one. Text selection positions on text
inputs count characters (Unicode scalar values), with UTF-8 byte offsets
alongside; lower-level text nodes report UTF-8 offsets, as the runtime does.
