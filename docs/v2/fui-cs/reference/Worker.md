# Worker

class in `FuiCs`: `IDisposable`

A one-shot background job in a Web Worker. [`Worker.Start`](Worker.md) runs the entry [`Worker.EntryName`](Worker.md) of a published fui-cs worker project with a string input; the worker reports progress, then completes with a result or fails. [`Worker.Path`](Worker.md) is the worker's published folder relative to the page (`browser/workers/Name` for a worker project the application references with `NetWasmWorker="true"`), or its client module.

Fluent members: [`WorkerExtensions`](WorkerExtensions.md).

## Properties

- `EntryName`: The entry the worker runs.
- `Path`: The worker app's path.

## Methods

- `Cancel()`: Asks the running worker to stop; its progress reports are no longer raised.
- `Dispose()`: Cancels a running worker and raises no further events.
- `Start(string)`: Starts the worker with `input`; an input over 1 MiB of UTF-8 raises [`Worker.Error`](Worker.md).

## Events

- `Completed`: Raised when the worker finishes.
- `Error`: Raised when the worker fails, including a cancelled worker that reports how it stopped.
- `Progress`: Raised for each progress report, until the worker finishes or is cancelled.
