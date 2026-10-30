# FuiCs.Worker

The worker-project library for FuiCs: a NetWasm `JsModule` with
`NetWasmJavaScriptHosts` set to `worker` references it, registers its entries
in a `FuiWorker` class, and runs them in a Web Worker for the page's `Worker`.

```csharp
public static class FuiWorker
{
    public static void Register(WorkerEntries entries) =>
        entries.Add("count", input => WorkerRuntime.Complete(input.Length.ToString()));
}
```

Docs: <https://github.com/zion-sati/fui-cs/blob/main/docs/v2/fui-cs/HOST_SERVICES_AND_WORKERS.md>

License: AGPL-3.0-only, or the commercial terms in the repository's
`v2/fui-cs/COMMERCIAL.md`.
