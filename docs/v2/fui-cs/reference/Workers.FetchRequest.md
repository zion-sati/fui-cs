# FetchRequest

class in `FuiCs.Workers`

An HTTP request a worker job sends through its Web Worker, as the page's `FetchRequest` does; a relative URL resolves against the worker's script.

Fluent members: [`FetchRequestExtensions`](Workers.FetchRequestExtensions.md).

## Properties

- `Body`: The request body; null sends none.
- `Headers`: The headers [`FetchRequest.AddHeader`](Workers.FetchRequest.md) added, in order.
- `IsSent`: Whether the request has been sent; a request is sent once.
- `Method`: The HTTP method. GET by default.
- `Url`: The URL to request; a relative URL resolves against the worker's script.

## Methods

- `AddHeader(string, string)`: Adds a header to send; a name added twice is sent twice.
- `SendAsync(CancellationToken)`: Sends the request; cancelling the token aborts it.
