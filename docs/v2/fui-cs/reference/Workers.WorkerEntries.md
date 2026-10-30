# WorkerEntries

class in `FuiCs.Workers`

The named entries a worker project offers; the page starts one with `new Worker(path, "name")`. The worker project registers them in `FuiWorker.Register(WorkerEntries entries)`.

## Methods

- `Add(string, Action<string>)`: Adds an entry that does all its work at once, without yielding.
- `Add(string, Func<string, Task>)`: Adds the entry `name`, an async job given the page's input; a later entry of the same name replaces it.
- `TryGet(string, out Func<string, Task>)`: Finds the entry `name`, as the worker does when the page starts it.
