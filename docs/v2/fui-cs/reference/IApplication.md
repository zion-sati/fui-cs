# IApplication

interface in `FuiCs`: `IDisposable`

A runnable application: [`IApplication.Run`](IApplication.md) builds and mounts its page, `IDisposable.Dispose` unmounts it.

## Methods

- `FlushRenders()`: Commits pending changes to the screen now.
- `Run()`: Builds and mounts the application's page.
