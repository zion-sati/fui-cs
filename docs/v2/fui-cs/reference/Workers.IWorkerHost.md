# IWorkerHost

interface in `FuiCs.Workers`

The worker's side of the page link, which the worker project's generated exports provide.

## Methods

- `CancelFetch(uint)`: Aborts the fetch `requestId`, whose task then faults.
- `FetchAsync(uint, string, string, string, byte[])`: Sends a fetch; the result is "ok\nstatus\nstatusText\nurl" and a failure faults the task.
- `ReportProgress(string)`: Sends a progress report to the page.
