# FUI-CS Quickstart

FUI-CS is the C# SDK for EffinDom v2. It compiles to WebAssembly with NetWasm
as a `JsModule` and drives the EffinDom retained-mode C++ UI runtime through the
browser bridge.

This page is a tour of the SDK. The [docs index](./SDK_INDEX.md) lists the
guides that cover each area in depth, and the
[API reference](./reference/README.md) every public type.

## Prerequisites

- The .NET SDK pinned in `global.json`
- Node.js 24+ and npm, which the build uses for the TypeScript side (it runs
  `npm ci` itself)

No .NET workloads are needed. The first restore downloads NetWasm's host tools
(Node, `wasm-ld`, `wasm-opt`) through NuGet.

## Build and run

From the repository root:

```bash
dotnet build
dotnet publish v2/fui-cs/demo/Home -c Release
dotnet publish v2/fui-cs/browser-app -c Release
```

The first publish writes the demo to `public/v2/fui-cs/demo/`, every route with
the shell and the runtime; the second writes the test pages to
`public/v2/fui-cs/`. Serve `public/` with any static server, for example
`npm run serve`, and open:

```text
http://127.0.0.1:8080/v2/fui-cs/demo/
http://127.0.0.1:8080/v2/fui-cs/index.html
```

## Tests

```bash
dotnet test
```

runs the unit tests and the browser tests. The unit tests drive the retained
node tree, event routing, drag and drop and the application shell against a
call-recording ABI. They are TUnit tests that run in two lanes: on desktop .NET
for fast iteration, and compiled to WebAssembly with NetWasm. The browser tests
publish the demo and the test pages, then run each Playwright spec in Chromium.

The standalone repository consumes `@effindomv2/runtime` from npm. It does not
contain or compile the EffinDOM runtime source.

## Applications From the Templates

An application of your own needs only the .NET SDK: the `FuiCs` package brings
the SDK, the web shell and, through `FuiCs.Runtime`, the EffinDOM runtime.

```bash
dotnet new install FuiCs.Templates
dotnet new fui-app -o MyApp
cd MyApp
dotnet publish -c Release -o site
```

`site/` is a static site; serve it at the root of any static file server.
`dotnet new fui-routed-app` creates a site with two routes and a library they
share (see [Routed Applications](#routed-applications)).

## Application Model

An application is a tree of retained `Node`s. Building a node creates its
native counterpart once; after that, setting a property updates the native node
and schedules a single commit for the frame. Input arrives as multicast C#
events on the nodes (`PointerClick`, `KeyDown`, `DragOver`, ...), routed and
bubbled the way the host reports it.

Every property has a fluent member of the same name that returns the node as
its own type, and every event has an `On...` member that subscribes:

```csharp
namespace MyApp;

static class FuiProgram
{
    public static IApplication CreateApplication() => Application.Create(BuildPage);

    static Node BuildPage()
    {
        var greeting = new Text("Hello").FontSize(20.0f);
        return FlexBox.Column(greeting)
            .Padding(24.0f)
            .Cursor(CursorStyle.Pointer)
            .OnPointerClick(e => greeting.Text = "Clicked");
    }
}
```

`FuiProgram.CreateApplication()` in the project's root namespace is the entry
point. The host starts the application through `__runApp`, which initializes
the runtime and runs the application `CreateApplication` returns. To use
another class, set `<FuiProgramType>MyApp.Startup</FuiProgramType>` in the
project. Without one, the build fails with CS0400 or CS0234 naming the missing
type in `FuiCs.Entry.g.cs`. `Application.Register(app => app.Page(BuildPage).UseSystemTheme())`
also returns the application, for apps that configure the theme at startup.

A `FlexBox` does not clip its children by default, as CSS's `overflow: visible`
does: content and drop shadows may extend past it, over what lies beneath. Set
`ClipToBounds` where overflow must be hidden, such as a rounded card that cuts
its image at the corners; scroll views always clip their content.

Text without an explicit color, and the application background, follow
`Theme.Active`; `Theme.UseSystem()` tracks the host's dark mode and accent.

`TextLayout` shapes text outside the page, for measuring or drawing. It is
ready once the page has loaded and its fonts are available:

<!-- example: FlexBox underline -->
```csharp
new TextLayout("left").FontSize(28.0f)
    .OnReady(e => underline.Width(e.Layout.MeasuredWidth));
```

Text nodes and layouts share the `ITextFormat` fluent members (`FontSize`,
`TextColor`, `MaxLines`, ...). `DynamicTextLayout` serves short text that
changes often, such as counters, from a fixed charset.

`ScrollView` is the native scroll viewport. Like WPF's `ScrollViewer` it
reports `HorizontalOffset`, `VerticalOffset`, the extent and the viewport, raises
`ScrollChanged` for host and programmatic scrolls, and moves with `ScrollTo`, or
`ScrollToAnimated` with an `AnimationTiming`. `FlexBox` and `ScrollView` share
the `Child`/`Children` fluent members through `IContentContainer`.

Animations run on the host's frames. `Animation.AnimateFloat` and
`Animation.AnimateColor` sample from one value to another over an
`AnimationTiming` (a duration and an `Easings` curve) and return the
`Animation`, which `Cancel` stops and `Finish` jumps to its end. A node's
`Transitions` animate later changes on a built node: `Opacity` and `Background`
on a `FlexBox`, and the offset set through `ScrollOffset`/`SetScrollOffset` on a
`ScrollView` or `ScrollBox`:

```csharp
var card = new FlexBox().Background(0x2563EBFF)
    .Transitions(new NodeTransitions { Opacity = new AnimationTiming(TimeSpan.FromMilliseconds(250), Easings.CubicInOut) });
card.Opacity = 0.2f; // fades over 250 ms once the card is built
```

A node's opacity fades the node and everything inside it together.
`Application.FrameTime` holds the last frame's timestamp, and
`Application.DevicePixelRatio` reads the display's current device pixel ratio.

`ScrollBox` wraps a `ScrollView` with scroll bars, like WPF's `ScrollViewer`:
content goes into the view, and `VerticalScrollBarVisibility` and
`HorizontalScrollBarVisibility` (`Auto`, `Visible`, `Hidden`) decide when each
bar shows. Dragging a thumb or pressing a track scrolls, and wheel input over
the bars scrolls the view. Each bar (`VerticalScrollBar`, `HorizontalScrollBar`)
restyles with its track and thumb thickness, minimum thumb length, corner radii
and colors, which follow the theme until set:

<!-- example: ScrollBox box -->
```csharp
box.VerticalScrollBar.TrackThickness(10.0f).ThumbThickness(6.0f).MinThumbLength(36.0f)
    .TrackCornerRadius(5.0f).ThumbCornerRadius(3.0f).ThumbColor(0x64748BFF);
```
 When nothing focused handles them, the arrow keys,
Page Up, Page Down, Home and End scroll the view around the focused node or the
one last clicked. A `SelectionArea` lets one drag select text across its nodes
(`SelectedText`), and an `AntiSelectionArea` keeps its text out of it.

`Image.Load(url)` and `Svg.Load(url, tint)` show host-loaded assets. An auto
width or height takes the asset's intrinsic size once it loads, keeping the
aspect ratio when the other side is fixed; `AssetStateChanged` reports loading,
ready and failed. A relative URL resolves against the page's address, so a
routed application keeps its assets in its route's folder.

`TextureAsset.Load(url)` and `SvgAsset.Load(url)` load an asset without a node,
for drawing (`DrawContext.DrawImage(texture, ...)`, `DrawSvg(svg, ...)`) or for
showing by `Id`. Everything holding a URL shares one host asset, released when
the last holder is disposed; `State`, `StateChanged`, `Width`, `Height` and
`Error` report it.

A `CustomDrawable` subclass draws in `OnRender(DrawContext)` (rectangles,
circles, lines, `PathGeometry` paths, text layouts, textures, SVGs and
transforms) and repaints after `InvalidateVisual()`. The drawing replaces the
box style, so draw any background yourself.

A `Bitmap` holds pixels you write (`Pixels`, then `Commit()`) or draw through
its `Canvas`, and shows as an image or through `DrawImage`. `Render(node)`
draws a built text node into it: build the node, wait for
`Bitmap.WhenTextReady(node, ...)`, then render from `OnRender`. `Render` returns
false until the node's prepared text has been committed, and after the node
changes (it is prepared again); render again on a later frame.

Platform services: `Platform` (host family, environment, capabilities),
`KeyboardShortcuts`, `Navigation.NavigateTo` and `Navigation.CurrentRoute`,
`await new FetchRequest(url).SendAsync()` for host requests (a request is
sent once: `IsSent` turns true and a second `SendAsync` throws), and
`PersistState(...)` for state restored when the user goes back. Timers are the
BCL's: `Task.Delay`, `PeriodicTimer` and `TimeProvider`.

`Button` raises `Click` for a primary click, or Enter or Space while focused;
a press released outside the button does not click, as in browsers and WPF.
Its chrome follows the theme; `Colors` (`ButtonColors`) and the chrome
properties (`Background`, `HoverBackground`, `Padding`, `TextColor`, ...)
override it, and a `ButtonTemplate` builds custom content around the label.

`Checkbox`, `RadioButton` and `Switch` pair an indicator with a label; a
click anywhere on them, or Space while focused, activates them. `IsChecked`
holds the value (a `bool?` on `Checkbox`, where null is indeterminate, which
only an `IsThreeState` checkbox keeps and cycles through) and `Changed` reports
every change.
A `RadioGroup` keeps one radio selected (`SelectedIndex`, `SelectedValue`) and
moves the selection with the arrow keys, Home and End. `LabeledControlColors`,
`LabeledControlSizing` and an `IndicatorTemplate` restyle the indicator.
Checkbox, switch and radio group values persist across history navigation.

`ProgressBar` fills in proportion to `Value` between `Minimum` and `Maximum`.
`Slider` picks a value by pressing or dragging along its track, or with the
arrow keys, Home and End; values snap to `Step` and `Changed` reports them.
Both have a `Length` and an `Orientation` (vertical increases upwards);
`SliderColors`, `SliderSizing` and a `SliderTemplate` restyle the slider.

A `Dropdown` picks one of its `Items` (`DropdownItem`s, each a value and a
label) from a list that opens under the field, or above it when only that fits.
A press, Enter, Space or the down arrow opens the list; the arrows, Home and End
move its highlight, Enter or a release on a row selects, and Escape, a press
outside or scrolling the field out of view closes it, while focus stays on the
field. `SelectedIndex` (-1 for none) sets the selection without raising
`Changed`, which reports the user's choices; assigning `Items` selects the
first. `MaxVisibleItems` limits the rows before the list scrolls, `PopupWidth`
widens it past the field, and the list keeps the highlighted row in view and
stays inside the viewport. `PopupPanelColor` and `PopupPanelBackgroundBlur`
restyle its panel. `DropdownFieldTemplate`, `DropdownChevronTemplate` and
`DropdownOptionRowTemplate` (each with a `Default`) build the field, its
chevron and the list's rows from `DropdownSizing`, and their presenters'
`Apply` styles them for each state with `DropdownColors`. The selection
persists across history navigation, and a restored selection raises `Changed`.
A `ComboBox` is a text field over a list of `Items` (`ComboBoxItem` values)
that filters as the user types (`FilterMode`: contains, starts with or none,
ignoring case) and opens, unless `StaysOpenOnEdit` is off. The arrows open it
and move its highlight, Enter or a release on a row selects (`Changed`), and
Escape, a press outside, blurring or scrolling the field away closes it.
`AllowCustomValue` decides whether text matching no item clears the selection,
`AutoComplete` completes the typed text with the first item that starts with
it, `CommitMode` keeps the text, reverts it to the selection or selects the
exact match when the list closes or the field blurs, and `OpenOnFocus` opens
the list on focus. `TextChanged` reports the user's edits; setting `Text` or
`SelectedIndex` from code raises nothing. It shares the dropdown's list
settings, `DropdownSizing`, `DropdownColors` and chevron and row templates.

`TextInput` is a single-line text field. Typing, pasting and the browser's
editing keys change its `Text`, and `TextChanged` reports each edit by the user
(setting `Text` from code moves the caret to the end and raises nothing).
Selection positions count characters (Unicode scalar values): `SelectionStart`
and `SelectionEnd`, `SetSelection`, `SetCaret` and `CaretToEnd`, with the same
positions in UTF-8 bytes in `SelectionStartByteOffset` and
`SelectionEndByteOffset`; `SelectionChanged` reports the user's moves. A
`Placeholder` shows while the text is empty and labels the field; `IsReadOnly`
allows selecting but not editing, `IsPassword` obscures the text and marks it
for the browser and password managers, `HostAutofill` passes an autofill hint
such as `username`, and `AcceptsTab` inserts tabs instead of moving focus.
`FontFamily`, `FontSize` and `LineHeight` follow the theme until set. The
control's `IsFocused` and `FocusChanged` follow its editor, `Focus()` restores
its selection, and the arrows, Home and End move the caret without scrolling
the page while shortcuts pass through. A `TextInputTemplate` builds the shell's
presenter, which `TextInputColors` restyles. The text persists across history
navigation unless it is a password. `MaxChars` sets the character limit, which
the runtime does not yet apply to typed text.
`TextArea` is the multi-line field on the same base: it sits in a scroll box,
wraps by default (`Wrapping`), scrolls sideways when it does not, and takes
`VerticalScrollBarVisibility` and `HorizontalScrollBarVisibility`; Enter adds a
line, and a press anywhere in it focuses the editor.

`Dialog` is a modal card with a `Title`, `Body`, optional `BodyContent` and OK
and Cancel buttons over a blurred backdrop. `Show()` opens it (or
`await dialog.ShowAsync()`, which returns whether it was accepted); Enter
accepts, Escape or a backdrop click cancels, `Accepted` and `Cancelled` report
the result, and focus returns to where it was. A `Form` with a `DefaultButton`
and `CancelButton` gives any page the same Enter and Escape keys while active.
A `Form` also groups text inputs for browser autofill and password managers:
the host projects each input with a `HostAutofill` hint inside a form into a
real form field, named after the input's `NodeId`, while it is on screen, and
autofilled values flow back into the inputs.

Right-clicking shows the built-in context menu: Copy and Select All over
selectable text; Undo, Redo, Cut, Copy, Paste and Select All over editable
text, such as a text input's field, returning focus to it after each; opening
an image; copying the page selection; and Back, Forward and Reload where the
host supports them. Under touch, a selection in editable text shows Cut, Copy,
Paste and Select all in the selection toolbar. A node's
`ContextMenuOpening` event replaces it (show your own `ContextMenu` of
`MenuItem`s, whose `Click` runs your code), and `IsContextMenuEnabled = false`
turns menus off for a subtree. To restyle the built-in menu, put a
`ContextMenu` in your tree and pass it to `ContextMenuManager.SetMenu`: the
built-in items then show in it, until you pass null or mount a new root.

`VirtualList` shows a long list of fixed-height items by building only the
rows in view: `new VirtualList(100_000, 32.0f).OnBindItem((row, index) => ...)`
fills a pooled row's container for an item index, creating its nodes the first
time and updating them after, as the list scrolls. `UpdateItemCount` changes the
count, and the list's `NodeId` and `PersistScroll` apply to its scroll box.

`TabView` shows one tab's content at a time and has no tab strip of its own:
build the selector you want and set `SelectedIndex`. Each `TabItem` has a
`Label`, `Description` and `IsEnabled`, and a factory for its content, a
`RetainedView` created the first time the tab is selected and deactivated,
not disposed, while another tab shows. `SelectionChanged` reports changes, and
removing or disabling the selected tab selects the nearest enabled one.

`Popup` shows any content next to an anchor or at a point over an optional
backdrop; `Opened` and `Closed` report when it shows and hides.

`ToolTip` sets a node's tooltip: `button.ToolTip("Save the file")`, or a
`new ToolTip(text) { ... }` with WPF's ToolTipService timing
(`InitialShowDelay`, `BetweenShowDelay`, `ShowDuration`), a `Placement` and
offsets, colors, and `ShowsOnKeyboardFocus`. It shows at the pointer after the
pointer rests on the node, or next to the node while it has visible keyboard
focus, and hides when the pointer leaves or presses, focus moves, the page
scrolls under it, or its duration runs out. The browser's find on page finds
an application's text, and its inspection tools read the host's debug tree.

`Cursor` sets the mouse cursor over a node that handles input. Buttons,
labeled controls, sliders, menu items and links show a pointer, selectable text
an I-beam and a scroll bar's thumb a grab hand; disabled controls show the
default. Selectable text inside a node that sets its own cursor, such as a
link's text, shows that node's cursor. A node that captured the pointer keeps
its cursor while dragging, and touch devices show no cursor.

`NavLink` makes any content a link to `Href`: a click, tap or Enter navigates,
and a middle click, Ctrl or ⌘, or `OpenInNewTab` opens a new tab. While it is
hovered or focused the host previews its URL, and its context menu offers New
Tab and Open. `Navigate` and `InteractionStateChanged` report activation and
hover, press and focus for restyling.

On touch, holding selectable text selects the word under the finger; handles
at the selection's ends drag to extend it, and a toolbar above it offers Copy
and Select all (items past three go behind More). Holding a link or image
offers its context menu items in the same toolbar, and a tap elsewhere closes
it. `SelectionHandles.Mode` shows the handles for touch selections (`Auto`,
also before any input on a coarse-pointer device), for every selection
(`Always`), or never (`Disabled`).

`Application.Mount` puts the root inside the application shell; replacing it
disposes the previous tree, cancels its requests still in flight and clears its
focus ring, selection chrome and tooltip, so start requests once the page has
loaded (`FrameScheduler.OnLoaded`). The host calls into the application through
exports (`__runApp`, `__flushRenders`, pointer, key and scroll callbacks) and
the browser ABI that the FuiCs build compiles into every NetWasm application
project.

## Routed Applications

A multi-route site makes each route its own application: a `JsModule` project
with its own `FuiProgram`, published to its own directory. The repository's
demo (`v2/fui-cs/demo/`) shows the shape: `routes.json` maps each route path
to its NetWasm module, and the shell harness runs the routed harness with an
instantiator that opens the module of the route being loaded. Same-origin
navigation (`Navigation.NavigateTo`) and browser history swap routes. A route
names its page with `Application.SetCaption`, which sets the page title.

A packaged application lists its other routes in its project file; the project
itself is the home route, at the site's root:

```xml
<ItemGroup>
  <FuiCsRoute Include="Routes/Settings/MyApp.Settings.csproj" RoutePath="/settings/" Title="Settings" />
</ItemGroup>
```

Publishing it publishes each route into the folder its path names, with the
routed harness, the route table in `fui-app.js` and a copy of the page in every
route's folder, so each address loads directly. A route's module is named after
its project file. `FuiCsRouteBase` sets the address the site is served at (`/`
by default) and `FuiCsHomeTitle` the home route's title.

## Host Services and Events

An application calls its page's JavaScript with .NET's `[JSImport]`: a host
service is a JavaScript function the application calls, and a host event a
callback the page raises in it. Write the JavaScript as a module and declare it
to NetWasm under the name the imports use; NetWasm bundles it into the
application, so the .NET SDK is still all a build needs:

```xml
<ItemGroup>
  <NetWasmJavaScriptModule Include="Host/shell.mjs" Module="app.shell" />
</ItemGroup>
```

```js
// Host/shell.mjs, which runs in the page.
export function formatBytes(count) {
  return `${(count / 1024).toFixed(1)} KB`;
}

export function onDarkModeChanged(callback) {
  const query = matchMedia('(prefers-color-scheme: dark)');
  const listener = (event) => callback(event.matches);
  query.addEventListener('change', listener);
  callback(query.matches);
  return { dispose() { query.removeEventListener('change', listener); } };
}
```

An event's import takes the callback and returns the subscription, which the
page ends when it unmounts:

<!-- example: skip -->
```csharp
using System.Runtime.InteropServices.JavaScript;

internal static class Shell
{
    [JSImport("formatBytes", "app.shell")]
    internal static extern string FormatBytes(double count);

    [JSImport("onDarkModeChanged", "app.shell")]
    internal static extern JSSubscription OnDarkModeChanged(Action<bool> handler);
}
```

<!-- example: skip -->
```csharp
var size = new Text(Shell.FormatBytes(1_048_576.0));
var mode = new Text("Light");
var subscription = Shell.OnDarkModeChanged(dark => mode.Text = dark ? "Dark" : "Light");
Application.OnUnmount(subscription.Dispose);
```

`Application.OnUnmount` runs once when the page goes away: when the
application closes, or a routed site swaps in another route. In a routed site,
put the imports and the module in the shared project: every route that
references it gets the module. Each route bundles its own copy, so state the
routes share lives on `globalThis`.
[Host services, host events and workers](./HOST_SERVICES_AND_WORKERS.md) has
the details.

## Workers

A `Worker` runs a background job in a Web Worker. The page names a published
worker project and one of its entries, starts it with a string input, and
receives progress, then one result or failure:

<!-- example: Text status, Text result, string input -->
```csharp
var worker = new Worker("workers/Reports", "summarize")
    .OnProgress(e => status.Text = e.Message + "%")
    .OnCompleted(e => result.Text = e.Result)
    .OnError(e => status.Text = "failed: " + e.Message)
    .Start(input);
// later, to stop it: worker.Cancel();
```

A worker runs one input. `Cancel` asks the job to stop; a job that notices
usually fails with a message saying how far it got. `Dispose`, and mounting a
new root, cancel a running worker and silence it. The path is the worker's
published folder relative to the page (`browser/workers/Reports` holds
`Reports.worker-client.mjs`), or the client module itself.

A worker project is a NetWasm worker that references `FuiCs.Worker` and
registers its entries in `FuiWorker.Register` (set `FuiWorkerType` to use
another class). The application references it with `NetWasmWorker="true"`,
so publishing the application publishes the worker into
`browser/workers/<Project>/`, where it shares the page's copy of the NetWasm
runtime:

```xml
<ItemGroup>
  <ProjectReference Include="..\Reports\Reports.csproj" NetWasmWorker="true" />
</ItemGroup>
```

```xml
<Project Sdk="NetWasm.Sdk">
  <PropertyGroup>
    <OutputType>JsModule</OutputType>
    <NetWasmJavaScriptHosts>worker</NetWasmJavaScriptHosts>
    <TargetFramework>netwasm0.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\FuiCs.Worker\FuiCs.Worker.csproj" />
  </ItemGroup>
</Project>
```

<!-- example: worker -->
```csharp
public static class FuiWorker
{
    public static void Register(WorkerEntries entries) => entries
        .Add("summarize", SummarizeAsync)
        .Add("index", input => new IndexJob().Start(input));

    private static async Task SummarizeAsync(string input)
    {
        for (var part = 0; part < 10; ++part)
        {
            if (WorkerRuntime.IsCancelled)
            {
                WorkerRuntime.Fail("cancelled at " + part);
                return;
            }
            WorkerRuntime.ReportProgress((part * 10).ToString());
            await WorkerRuntime.YieldAsync();
        }
        WorkerRuntime.Complete("summary");
    }
}

internal sealed class IndexJob : WorkerJob
{
    private string text = "";

    protected override void OnStart(string input) => text = input;

    protected override async Task Run()
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        ReportProgress(words.Length + " words");
        if (await YieldAsync() && !IsCancelled)
        {
            Complete(string.Join("\n", words));
        }
        else
        {
            Fail("cancelled");
        }
    }
}
```

`WorkerRuntime` reports progress, then completes or fails once; a job that
throws, or returns without completing, fails. A long job awaits
`WorkerRuntime.YieldAsync` between slices so the worker sees its cancellation;
`WorkerJob` keeps a job's state in fields, with `OnStart(input)` and an
async `Run()`. Inside a job, `new FetchRequest(url).SendAsync()` (in
`FuiCs.Workers`) sends a request from the worker, resolving a relative URL
against the worker's script.

A worker can call JavaScript of its own, which runs inside the worker: its
project declares a module the same way, exporting `createWorkerImports()`
([details](./HOST_SERVICES_AND_WORKERS.md#a-workers-javascript)).

## Publish Shape

The app is a NetWasm `JsModule` built with `NetWasmOptimization=Oz` for the
smallest output. A page loads:

- `browser/FuiCs.Browser.wasm`, the compiled C# app;
- `browser/netwasm.page.mjs` and the `browser/netwasm.runtime.*.mjs` chunks, NetWasm's page runtime, generated from what the app links (about 46 KB Brotli for a demo route) and carrying the app's JavaScript import modules; a worker the app references shares them;
- `harness.js`, the EffinDom managed harness, which runs the C# module as the
  page's Tier 3 app.
