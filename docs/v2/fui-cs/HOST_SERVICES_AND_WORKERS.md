# FUI-CS Host Services, Host Events and Workers

An application reaches its page through .NET's JavaScript interop: **host
services** are JavaScript functions the application calls, **host events** are
callbacks the page raises in the application, and **workers** run C# jobs in
Web Workers. The [Quickstart](./QUICKSTART.md#host-services-and-events) shows
each briefly; this page covers the details.

## The page's JavaScript

Write the page's side as an ES module and declare it in the application
project, under the module name its imports use:

```xml
<ItemGroup>
  <NetWasmJavaScriptModule Include="Host/shell.mjs" Module="app.shell" />
</ItemGroup>
```

NetWasm bundles the module into the application, so a build still needs only
the .NET SDK, and the module runs in the page, with the DOM. Give modules
names of the application's own (`app.shell`); FuiCs's modules are named
`effindom_v2_ui`, `fui_host` and the like.

## Host services

A host service is a static `extern` method marked `[JSImport]` with the
function's name and the module's:

```js
// Host/shell.mjs
export function formatBytes(count) {
  return `${(count / 1024).toFixed(1)} KB`;
}
```

<!-- example: skip -->
```csharp
using System.Runtime.InteropServices.JavaScript;

internal static class Shell
{
    [JSImport("formatBytes", "app.shell")]
    internal static extern string FormatBytes(double count);
}
```

Arguments and results are `bool`, integers, `char`, `float` and `double`,
`string`, one-dimensional arrays of numbers (`byte[]` to `double[]`) and
`JSObject`; 64-bit integers are `bigint` in JavaScript, and an import that
returns `Task` or `Task<T>` awaits a JavaScript promise. NetWasm's
[supported signatures](https://github.com/zion-sati/NetWasm/blob/main/docs/javascript-interop.md#supported-signatures)
list the rest.

## Host events

A host event is an import that takes a callback and returns a
`JSSubscription`. The JavaScript keeps the callback, calls it as the event
happens, and returns an object whose `dispose()` stops it; calling it once
straight away gives the application the current value:

```js
export function onDarkModeChanged(callback) {
  const query = matchMedia('(prefers-color-scheme: dark)');
  const listener = (event) => callback(event.matches);
  query.addEventListener('change', listener);
  callback(query.matches);
  return { dispose() { query.removeEventListener('change', listener); } };
}
```

<!-- example: skip -->
```csharp
[JSImport("onDarkModeChanged", "app.shell")]
internal static extern JSSubscription OnDarkModeChanged(Action<bool> handler);
```

A callback's parameters are `bool`, integers, `char`, `float` or `double`,
with at most one `string` or number array. Setting a node's properties from it
draws them in the next frame, as from any other handler.

## Ending subscriptions

A page ends its subscriptions when it unmounts. `Application.OnUnmount`
runs a handler once when the page goes away: when the application closes, a
routed site swaps in another route, or `Application.Mount` replaces the page.
A handler registered while the page builds belongs to that page:

<!-- example: skip -->
```csharp
var mode = new Text("Light");
var subscription = Shell.OnDarkModeChanged(dark => mode.Text = dark ? "Dark" : "Light");
Application.OnUnmount(subscription.Dispose);
```

A subscription left running keeps an unloaded route's application in memory
and goes on calling into it.

## Routed sites

Put the imports and the module in the routed site's shared project, and
declare the module there: every route that references the shared project,
directly or through another library, gets the module. Each route bundles a
copy of its own, so JavaScript state the routes share, such as a timer, lives
on `globalThis`:

```js
const shell = (globalThis.appShell ??= { tick: 0, listeners: new Set() });
shell.timer ??= setInterval(() => {
  shell.tick += 1;
  for (const listener of shell.listeners) listener(shell.tick);
}, 1000);

export function onTick(callback) {
  shell.listeners.add(callback);
  return { dispose() { shell.listeners.delete(callback); } };
}
```

## Workers

A `Worker` runs a C# job in a Web Worker, keeping long work off the page's
thread:

<!-- example: Text status, Text result, string input -->
```csharp
var worker = new Worker("workers/Reports", "summarize")
    .OnProgress(e => status.Text = e.Message + "%")
    .OnCompleted(e => result.Text = e.Result)
    .OnError(e => status.Text = "failed: " + e.Message)
    .Start(input);
```

- The path is the worker's published folder relative to the page
  (`browser/workers/Reports` holds `Reports.worker-client.mjs`), or the client
  module itself.
- A worker runs one string input (up to 1 MiB of UTF-8; more raises `Error`);
  starting it twice throws. Create another `Worker` to run again.
- `Progress` reports any number of times, then exactly one of `Completed` or
  `Error`.
- `Cancel()` asks the job to stop and silences its progress. `Dispose()`, and
  mounting a new root, cancel a running worker and silence it.

### The worker project

A worker project is a NetWasm `JsModule` for the `worker` host that references
`FuiCs.Worker` (namespace `FuiCs.Workers`) and registers its entries in a
`FuiWorker` class (set `<FuiWorkerType>` to use another):

<!-- example: worker -->
```csharp
public static class FuiWorker
{
    public static void Register(WorkerEntries entries) => entries
        .Add("summarize", SummarizeAsync)
        .Add("count", input => WorkerRuntime.Complete(input.Length.ToString()));

    private static async Task SummarizeAsync(string input)
    {
        var lines = input.Split('\n');
        for (var index = 0; index < lines.Length; ++index)
        {
            if (!await WorkerRuntime.YieldAsync() || WorkerRuntime.IsCancelled)
            {
                WorkerRuntime.Fail("cancelled at line " + index);
                return;
            }
            WorkerRuntime.ReportProgress((index * 100 / lines.Length).ToString());
        }
        WorkerRuntime.Complete(lines.Length + " lines");
    }
}
```

An entry is an async function of the input (`Func<string, Task>`), a
synchronous one (`Action<string>`), or a `WorkerJob`. Inside it,
`WorkerRuntime` talks to the page:

| Member | Meaning |
|---|---|
| `ReportProgress(text)` | Raises the page's `Progress` |
| `Complete(result)` | Raises `Completed`; later results and failures are ignored |
| `Fail(message)` | Raises `Error`; later results and failures are ignored |
| `IsCancelled` | Whether the page has cancelled; seen when the job yields |
| `await YieldAsync(delay)` | Lets the worker see a cancellation between slices; false once the job has finished |

A job that throws, or returns without completing, fails with its message.
`WorkerJob` keeps a job's state in fields: `OnStart(input)` receives the
input and `Run()` does the work, with the same members available on the job.

The application references the worker project with `NetWasmWorker="true"`, so
publishing the application publishes the worker into
`browser/workers/<Project>/`, where it shares the page's copy of the NetWasm
runtime:

```xml
<ProjectReference Include="..\Reports\Reports.csproj" NetWasmWorker="true" />
```

### Fetching from a worker

`new FetchRequest(url).SendAsync()` from `FuiCs.Workers` sends a request
from the worker. A relative URL resolves against the worker's script; set
`Method`, headers (`Header(name, value)`) and a body (`Body(bytes)` or
`Body(text)`) fluently, and pass a `CancellationToken` to abort it. The
response (`FetchResponse`) carries the status and final URL; a network failure
throws `FetchException`.

### A worker's JavaScript

A worker project declares modules of its own the same way; they run inside the
worker, without the page's DOM. The module exports `createWorkerImports()`,
which returns the functions the worker imports:

```js
// Host/report.mjs
export function createWorkerImports() {
  return {
    formatBytes(count) {
      return `${(count / 1024).toFixed(1)} KB`;
    },
  };
}
```

```xml
<ItemGroup>
  <NetWasmJavaScriptModule Include="Host/report.mjs" Module="report.host" />
</ItemGroup>
```

The worker's C# imports them as a page does (`[JSImport("formatBytes",
"report.host")]`).

The full worker API is in the reference under
[Worker projects](./reference/README.md#worker-projects-fuicsworker).
