# WorkerRuntime

class in `FuiCs.Workers`

The running worker job's link to the page: progress reports, then one result or failure. Long jobs await [`WorkerRuntime.YieldAsync`](Workers.WorkerRuntime.md) between slices so the worker can see [`WorkerRuntime.IsCancelled`](Workers.WorkerRuntime.md).

## Properties

- `Host`: The running job's host, for the worker's fetch requests.
- `IsCancelled`: Whether the page has cancelled the job; seen when the job yields.

## Methods

- `Cancel()`: Cancels the running job, from the page's cancel call; the job sees it when it next yields.
- `Complete(string)`: Completes the job with `result`; later results and failures are ignored.
- `Fail(string)`: Fails the job with `message`; later results and failures are ignored.
- `ReportProgress(string)`: Reports progress; ignored once the job has completed or failed.
- `RunAsync(IWorkerHost, Func<string, Task>, string)`: Runs `entry` as the job and returns its result; a failure, an exception, and returning without a result each throw [`WorkerJobException`](Workers.WorkerJobException.md) with the job's message.
- `YieldAsync(TimeSpan)`: Lets the worker handle the page's calls (a cancellation) before the job continues, after `delay` if one is given. Returns false once the job has completed or failed.
