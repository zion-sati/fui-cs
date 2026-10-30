# FetchRequest

class in `FuiCs`

An HTTP request sent through the host. It reports the response status, not the body; use an HTTP client for responses you need to read.

Fluent members: [`FetchRequestExtensions`](FetchRequestExtensions.md).

## Properties

- `Body`: The request body; null sends none.
- `Headers`: The headers added so far, in order.
- `IsSent`: Whether the request has been sent; a request is sent once.
- `Method`: The HTTP method; GET until set.
- `Url`: The request's URL.

## Methods

- `AddHeader(string, string)`: Adds a header.
- `SendAsync(CancellationToken)`: Sends the request; cancelling the token cancels it in the host.
