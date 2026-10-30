# WorkerJob

class in `FuiCs.Workers`

A worker job with state: [`WorkerJob.OnStart`](Workers.WorkerJob.md) receives the input, then [`WorkerJob.Run`](Workers.WorkerJob.md) does the work, awaiting [`WorkerJob.YieldAsync`](Workers.WorkerJob.md) between slices. Register it with `entries.Add("name", input => new MyJob().Start(input))`.

## Properties

- `IsCancelled`: Whether the page has cancelled the job; seen when the job yields.

## Methods

- `Complete(string)`: Completes the job with `result`; later results and failures are ignored.
- `Fail(string)`: Fails the job with `message`; later results and failures are ignored.
- `OnStart(string)`: Receives the page's input before [`WorkerJob.Run`](Workers.WorkerJob.md); completing or failing here skips the run.
- `ReportProgress(string)`: Reports progress to the page; ignored once the job has completed or failed.
- `Run()`: Does the job's work, then completes or fails it; returning without either fails the job.
- `Start(string)`: Starts the job with `input` and runs it to its end.
- `YieldAsync(TimeSpan)`: Lets the worker handle the page's calls (a cancellation) before the job continues, after `delay` if one is given. Returns false once the job has completed or failed.
