# Logger

class in `FuiCs`

Categorized logging through the host.

## Properties

- `IsEnabled`: Whether the host records [`Logger.Log`](Logger.md) messages. Warnings and errors are always recorded.

## Methods

- `Error(string, string)`: Logs an error under `category`.
- `Log(string, string)`: Logs `message` under `category` to the host's console.
- `Warn(string, string)`: Logs a warning under `category`.
