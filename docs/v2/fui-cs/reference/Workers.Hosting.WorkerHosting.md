# WorkerHosting

class in `FuiCs.Workers.Hosting`

Runs the worker's entries for its generated exports.

## Methods

- `Cancel()`: Cancels the running job, for the page's cancel call; the job sees it when it next yields.
- `Run(IWorkerHost, Func<WorkerEntries>, string, string)`: Runs `entryName` with `input` and returns the job's result; a failed job, and an unknown entry, throw [`WorkerJobException`](Workers.WorkerJobException.md). Each run is the only job of its Web Worker, so the entries are registered for it.
