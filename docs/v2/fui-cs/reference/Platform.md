# Platform

class in `FuiCs`

Queries about the host, read once per runtime.

## Properties

- `Current`: The host's context, read once.
- `Environment`: Where the application runs: a browser, a desktop host or headless.
- `Family`: The platform family: Apple, Windows, Linux or unknown.
- `IsCoarsePointer`: Whether the primary pointer is coarse (touch). The host can change this, so it is read each time.

## Methods

- `Supports(HostCapability)`: Whether the host supports `capability`.
